using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RequestWrap
{
    public class APIwrapper : IRequestWrapper
    {
        private string _baseUrl;
        private int _timeoutInSeconds = 2000;

        private readonly List<Action<IRequestWrapper>> _preRequests;
        private readonly List<Func<string, string>> _endpointModifiers;
        private readonly List<Predicate<string>> _endpointValidators;

        private DelegatingHandler _delegatingHandler;
        private bool _changedHandler;

        private List<Task<string>> tasks = new List<Task<string>>();

        public APIwrapper(string baseUrl)
        {
            _baseUrl = baseUrl;
            _changedHandler = true;
            _preRequests = new List<Action<IRequestWrapper>>();
            _endpointValidators = new List<Predicate<string>>();
            _endpointModifiers = new List<Func<string, string>>();
        }

        public HttpClient HTTPclient { get; private set; }

        private void checkAndResetHTTPClient()
        {
            if (HTTPclient == null || _changedHandler)
            {
                HTTPclient = HttpClientFactory.Create(_delegatingHandler ?? new defaultHandler());
                HTTPclient.BaseAddress = new Uri(_baseUrl);
                HTTPclient.Timeout = new TimeSpan(0, 0, 0, _timeoutInSeconds);
                _changedHandler = false;
            }
        }

        private void runEndpointValidators(string endpointURI)
        {
            if (_endpointValidators.Any(validator => !validator(endpointURI)))
            {
                throw new ArgumentException("Invalid endpoint");
            }
        }

        private void runPreRequest()
        {
            foreach (Action<IRequestWrapper> PreRequest in _preRequests)
            {
                PreRequest(this);
            }
        }

        public async Task<string> get(string endPointURI)
        {

            checkAndResetHTTPClient();

            runEndpointValidators(endPointURI);

            runPreRequest();

            var response = await HTTPclient.GetAsync(endPointURI);

            return response.Content.ReadAsStringAsync().Result;
        }

        public async Task<string> post(string endpointURI, string json)
        {
            checkAndResetHTTPClient();

            runEndpointValidators(endpointURI);

            runPreRequest();

            var response = await HTTPclient.PostAsync(endpointURI, new StringContent(json));

            return response.Content.ReadAsStringAsync().Result;
        }

        public int scheduleGet(string endpointURI)
        {
            throw new NotImplementedException("schedule get not implemented");
        }

        public bool executeTasks()
        {
            throw new NotImplementedException("execute tasks not implemented");
        }

        public IRequestWrapper addPreRequest(Action<IRequestWrapper> actionName)
        {
            _preRequests.Add(actionName);
            return this;
        }

        public IRequestWrapper clearPreRequest()
        {
            _preRequests.Clear();
            return this;
        }

        public IRequestWrapper endPointValidator(Predicate<string> validator)
        {
            _endpointValidators.Add(validator);
            return this;
        }

        public IRequestWrapper clearEndPointValidator()
        {
            _endpointValidators.Clear();
            return this;
        }

        public IRequestWrapper endPointModifier(Func<string, string> modifier)
        {
            _endpointModifiers.Add(modifier);
            return this;
        }

        public IRequestWrapper clearEndPointModifier()
        {
            _endpointModifiers.Clear();
            return this;
        }

        public IRequestWrapper setTimeout(int seconds)
        {
            _timeoutInSeconds = seconds;
            return this;
        }

        public IRequestWrapper setMessageHandler(DelegatingHandler handler)
        {
            _delegatingHandler = handler;
            _changedHandler = true;
            return this;
        }

        public IRequestWrapper setNullHandler()
        {
            _delegatingHandler = new NullHandler();
            _changedHandler = true;
            return this;
        }

        public IRequestWrapper resetHandler()
        {
            _delegatingHandler = null;
            _changedHandler = true;
            return this;
        }

    }

    internal class NullHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(request.ToString())
            };

            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response);
            return tsc.Task;
        }
    }

    internal class defaultHandler : DelegatingHandler
    {
        
    }
}
