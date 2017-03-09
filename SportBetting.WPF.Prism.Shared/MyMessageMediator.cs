
using System;
using System.Collections.Generic;
using System.Linq;
using SharedInterfaces;
using SportRadar.Common.Logs;

namespace SportBetting.WPF.Prism.Shared
{
    public class MyMessageMediator : IMediator
    {


        private static ILog Log = LogFactory.CreateLog(typeof(MyMessageMediator));

        public Dictionary<Type, List<IActionDetails>> _registeredHandlers = new Dictionary<Type, List<IActionDetails>>();

        #region Methods



        public bool Register(IActionDetails action)
        {

            var messageType = action.Type;

            if (IsRegistered(action))
            {
                Log.WarnFormat("Same handler for message type '{0}' with tag '{1}' is already registered, skipping registration", messageType.Name, action.MsgTag);

                return false;
            }

            if (!_registeredHandlers.ContainsKey(messageType))
            {
                _registeredHandlers.Add(messageType, new List<IActionDetails>());
            }
            var list = _registeredHandlers[messageType];
            list.Add(action);

            Log.DebugFormat("Registered handler for message type '{0}' with tag '{1}'", messageType.Name, action.MsgTag);

            return true;
        }


        public bool UnregisterRecipientAndIgnoreTags(object recipient)
        {
            return UnregisterRecipient(recipient, null, true);
        }


        public bool UnregisterRecipient(object recipient, object tag, bool ignoreTag)
        {

            int handlerCounter = 0;
            var keys = _registeredHandlers.Keys.ToList();
            foreach (var key in keys)
            {
                var messageHandlers = _registeredHandlers[key];
                for (int i = 0; i < messageHandlers.Count; i++)
                {
                    var handlerInfo = messageHandlers[i];
                    if (ignoreTag || String.Equals((string)tag, handlerInfo.MsgTag))
                    {
                        if (handlerInfo.ViewModel == recipient)
                        {
                            messageHandlers.RemoveAt(i--);
                            handlerCounter++;

                            Log.DebugFormat("Unregistered handler for message type '{0}' with tag '{1}' from '{2}'", key.Name, handlerInfo.MsgTag, handlerInfo.ViewModel.ToString());
                        }
                    }
                }
            }

            Log.DebugFormat("Unregistered '{0}' handlers for the recipient from '{1}'", handlerCounter, recipient.ToString());

            return true;
        }


        public bool SendMessage<TMessage>(TMessage message, string tag)
        {

            Log.DebugFormat("Sending message of type '{0}' value '{2}' with tag '{1}'", message.GetType().FullName, tag, message);

            int invokedHandlersCount = 0;

            var messageType = typeof(TMessage);

            if (_registeredHandlers.ContainsKey(messageType))
            {
                var _registeredHandlersCopy = new List<IActionDetails>();
                foreach (var item in _registeredHandlers[messageType])
                {
                    _registeredHandlersCopy.Insert(0, item);
                }
                for (int index = 0; index < _registeredHandlersCopy.Count; index++)
                {
                    var handler = _registeredHandlersCopy[index];
                    if (String.Equals(tag, handler.MsgTag) && handler.Execute(message))
                    {
                        ++invokedHandlersCount;
                    }
                }
            }

            Log.DebugFormat("Sent message to {0} recipients", invokedHandlersCount);

            return invokedHandlersCount != 0;

        }


        public bool IsRegistered(IActionDetails details)
        {

            var messageType = details.Type;

            if (_registeredHandlers.ContainsKey(messageType))
            {
                var messageHandlers = _registeredHandlers[messageType];
                for (int i = 0; i < messageHandlers.Count; i++)
                {
                    var handlerInfo = messageHandlers[i];


                    if (!ReferenceEquals(details.ViewModel, handlerInfo.ViewModel))
                    {
                        continue;
                    }

                    if (string.Equals(details.MsgTag, handlerInfo.MsgTag) && string.Equals(details.MethodName, handlerInfo.MethodName))
                    {
                        return true;
                    }
                }
            }

            return false;
        }






        #endregion
    }
}