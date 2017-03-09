using System;

namespace SharedInterfaces
{
    public interface IMediator
    {
        bool UnregisterRecipientAndIgnoreTags(object recipient);
        bool SendMessage<TMessage>(TMessage message, string tag);
        bool Register(IActionDetails actionDetails);
    }

    public interface IActionDetails
    {
        IClosable ViewModel { get; set; }

        bool Execute(object value);

        string MsgTag { get; set; }

        Type Type { get; set; }

        string MethodName { get; }
    }
}