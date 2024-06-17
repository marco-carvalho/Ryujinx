using Ryujinx.Common;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Ryujinx.HLE.Exceptions
{
    [Serializable]
    internal class ServiceNotImplementedException : Exception
    {
        public IpcService Service { get; }
        public ServiceCtx Context { get; }
        public IpcMessage Request { get; }

        public ServiceNotImplementedException(IpcService service, ServiceCtx context)
            : this(service, context, "The service call is not implemented.") { }

        public ServiceNotImplementedException(IpcService service, ServiceCtx context, string message) : base(message)
        {
            Service = service;
            Context = context;
            Request = context.Request;
        }

        public ServiceNotImplementedException(IpcService service, ServiceCtx context, string message, Exception inner) : base(message, inner)
        {
            Service = service;
            Context = context;
            Request = context.Request;
        }

        public override string Message
        {
            get
            {
                return base.Message + Environment.NewLine + Environment.NewLine + BuildMessage();
            }
        }

        private string BuildMessage()
        {
            StringBuilder sb = new();

            // Print the IPC command details (service name, command ID, and handler)
            (Type callingType, MethodBase callingMethod) = WalkStackTrace(new StackTrace(this));

            if (callingType != null && callingMethod != null)
            {
                // If the type is past 0xF, we are using TIPC
                var ipcCommands = Request.Type > IpcMessageType.TipcCloseSession ? Service.TipcCommands : Service.CmifCommands;

                // Find the handler for the method called
                var ipcHandler = ipcCommands.FirstOrDefault(x => x.Value == callingMethod);
                var ipcCommandId = ipcHandler.Key;
                var ipcMethod = ipcHandler.Value;

                if (ipcMethod != null)
                {
                    sb.AppendLine($"Service Command: {Service.GetType().FullName}: {ipcCommandId} ({ipcMethod.Name})");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("Guest Stack Trace:");
            sb.AppendLine(Context.Thread.GetGuestStackTrace());

            // Print buffer information
            if (Request.PtrBuff.Count > 0 ||
                Request.SendBuff.Count > 0 ||
                Request.ReceiveBuff.Count > 0 ||
                Request.ExchangeBuff.Count > 0 ||
                Request.RecvListBuff.Count > 0)
            {
                sb.AppendLine("Buffer Information:");

                if (Request.PtrBuff.Count > 0)
                {
                    sb.AppendLine("\tPtrBuff:");

                    for (int i = 0; i < Request.PtrBuff.Count; i++)
                    {
                        IpcPtrBuffDesc buff = Request.PtrBuff[i];
                        sb.AppendLine($"\t[{buff.Index}] Position: 0x{buff.Position:x16} Size: 0x{buff.Size:x16}");
                    }
                }

                if (Request.SendBuff.Count > 0)
                {
                    sb.AppendLine("\tSendBuff:");

                    for (int i = 0; i < Request.SendBuff.Count; i++)
                    {
                        IpcBuffDesc buff = Request.SendBuff[i];
                        sb.AppendLine($"\tPosition: 0x{buff.Position:x16} Size: 0x{buff.Size:x16} Flags: {buff.Flags}");
                    }
                }

                if (Request.ReceiveBuff.Count > 0)
                {
                    sb.AppendLine("\tReceiveBuff:");

                    for (int i = 0; i < Request.ReceiveBuff.Count; i++)
                    {
                        IpcBuffDesc buff = Request.ReceiveBuff[i];
                        sb.AppendLine($"\tPosition: 0x{buff.Position:x16} Size: 0x{buff.Size:x16} Flags: {buff.Flags}");
                    }
                }

                if (Request.ExchangeBuff.Count > 0)
                {
                    sb.AppendLine("\tExchangeBuff:");

                    for (int i = 0; i < Request.ExchangeBuff.Count; i++)
                    {
                        IpcBuffDesc buff = Request.ExchangeBuff[i];
                        sb.AppendLine($"\tPosition: 0x{buff.Position:x16} Size: 0x{buff.Size:x16} Flags: {buff.Flags}");
                    }
                }

                if (Request.RecvListBuff.Count > 0)
                {
                    sb.AppendLine("\tRecvListBuff:");

                    for (int i = 0; i < Request.RecvListBuff.Count; i++)
                    {
                        IpcRecvListBuffDesc buff = Request.RecvListBuff[i];
                        sb.AppendLine($"\tPosition: 0x{buff.Position:x16} Size: 0x{buff.Size:x16}");
                    }
                }

                sb.AppendLine();
            }

            sb.AppendLine("Raw Request Data:");
            sb.Append(HexUtils.HexTable(Request.RawData));

            return sb.ToString();
        }

        private static (Type, MethodBase) WalkStackTrace(StackTrace trace)
        {
            int i = 0;

            StackFrame frame;

            // Find the IIpcService method that threw this exception
            while ((frame = trace.GetFrame(i++)) != null)
            {
                var method = frame.GetMethod();
                var declType = method.DeclaringType;

                if (typeof(IpcService).IsAssignableFrom(declType))
                {
                    return (declType, method);
                }
            }

            return (null, null);
        }
    }
}
