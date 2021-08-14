using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaStreaming.Handlers.API
{
    public abstract class ApiHandler : Handler
    {
        protected abstract string ServiceName { get; }
        /// <summary>
        /// All possible values of the parameter "m". If this list is empty, then the "m" parameter is optional.
        /// </summary>
        protected Dictionary<string, Action> ModeParameters { get; } = new Dictionary<string, Action>();

        protected virtual void Other() { }
        protected abstract void Init();
        protected string Mode { get; private set; }

        public ApiHandler()
        {
            Init();
        }

        public override string Path => "api";

        public override bool RequireWebSocket => false;

        public override bool IsValidPath(string rootPath, string path)
        {
            var tmp = path.Split('/').Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
            string myPath = "";
            for (int i = 0; i < tmp.Length; i++)
            {
                myPath += $"/{tmp[i]}";
                if (myPath == $"{rootPath}/{Path}/{ServiceName}")
                    return true;
            }
            return false;
        }
        protected override void Execute()
        {
            Mode = HttpContext.Request.Query["m"];
            if (!checkMode())
            {
                Other();
                return;
            }
            ModeParameters[Mode].Invoke();
        }

        private bool checkMode()
        {
            if (ModeParameters != null && ModeParameters.Count > 0)
            {
                if (ModeParameters.Any(p => p.Key == Mode))
                    return true;
                sendHttpResponse($"Incorrect parameter 'm'!\n Expected: \"{string.Join("', '", ModeParameters.ToArray())}\"");
                return false;
            }
            return true;
        }
    }
}
