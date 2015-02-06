﻿using IronFoundry.Container.Messaging;
using Newtonsoft.Json.Linq;

namespace IronFoundry.Container.Messages
{
    public class StopAllProcessesParams
    {
        public int timeout;
    }

    public class StopAllProcessesRequest : JsonRpcRequest<StopAllProcessesParams>
    {
        public static string MethodName = "Container.StopAllProcesses";

        public StopAllProcessesRequest(StopAllProcessesParams @params)
            : base(MethodName)
        {
            this.@params = @params;
        }
    }

    public class StopAllProcessesResponse : JsonRpcResponse
    {
        public StopAllProcessesResponse(JToken id)
            : base(id)
        {
        }
    }
}
