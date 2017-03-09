using System;
using System.Collections.Generic;
using IocContainer;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;

namespace BaseObjects.ViewModels
{
    public class MessageStorage : IMessageStorage
    {

        private IMediator _messageMediator;
        public IMediator InternalMediator
        {
            get { return _messageMediator ?? (_messageMediator = IoCContainer.Kernel.Get<IMediator>()); }
        }

        public IList<IActionDetails> actions = new List<IActionDetails>();

        public bool SendMessage<T>(T message, string MsgTag)
        {
            return InternalMediator.SendMessage(message, MsgTag);
        }
        public bool Register<T>(IClosable viewModelBase, Action<T> action, string msgTag)
        {
            var type = viewModelBase.GetType();
            foreach (var actionDetailse in actions)
            {
                if (actionDetailse.ViewModel.GetType() == type && actionDetailse.ViewModel.IsClosed && actionDetailse.MethodName == action.Method.Name && actionDetailse.Type == typeof(T) && actionDetailse.MsgTag == msgTag)
                {
                    actionDetailse.ViewModel = viewModelBase;
                    ((ActionDetails<T>)actionDetailse).Action = action;

                }
            }

            foreach (var actionDetailse in actions)
            {
                if (actionDetailse.ViewModel == viewModelBase && actionDetailse.MethodName == action.Method.Name && actionDetailse.Type == typeof(T) && actionDetailse.MsgTag == msgTag)
                {
                    return false;
                }
            }


            var actionDetails = new ActionDetails<T>();
            actionDetails.ViewModel = viewModelBase;
            actionDetails.Action = action;
            actionDetails.Type = typeof(T);
            actionDetails.MsgTag = msgTag;
            actions.Add(actionDetails);
            return InternalMediator.Register(actionDetails);

        }

        public void UnregisterRecipientAndIgnoreTags(IClosable viewModel)
        {
            InternalMediator.UnregisterRecipientAndIgnoreTags(viewModel);
            actions.Clear();
        }

        public void ApplyRegistration()
        {
            foreach (var actionDetail in actions)
            {
                InternalMediator.Register(actionDetail);
            }
        }
    }

    public interface IMessageStorage
    {
        bool SendMessage<T>(T message, string MsgTag);
        void UnregisterRecipientAndIgnoreTags(IClosable viewModel);
        bool Register<T>(IClosable viewModelBase, Action<T> action, string msgTag);
    }
}