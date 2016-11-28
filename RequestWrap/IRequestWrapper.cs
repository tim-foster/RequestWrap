using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RequestWrap
{
    public interface IRequestWrapper
    {
        HttpClient HTTPclient { get; }

        Task<string> get(string endpointURI);
        Task<string> post(string endpointURI, string json);

        #region future improvements
        int scheduleGet(string endpointURI);

        #endregion

        IRequestWrapper addPreRequest(Action<IRequestWrapper> actionName);
        IRequestWrapper clearPreRequest();

        IRequestWrapper endPointValidator(Predicate<string> validator);
        IRequestWrapper clearEndPointValidator();

        IRequestWrapper endPointModifier(Func<string, string> endPointURI);
        IRequestWrapper clearEndPointModifier();

        #region HTTP Client specific properties

        IRequestWrapper setTimeout(int seconds);
        
        #endregion

        #region Message Handler Methods

        IRequestWrapper setMessageHandler(DelegatingHandler handler);
        IRequestWrapper setNullHandler();
        IRequestWrapper resetHandler();
        
        #endregion

    }
}