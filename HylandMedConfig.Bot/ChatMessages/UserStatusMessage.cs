using System;
using System.Collections.Generic;

namespace HylandMedConfig.Common
{
    [Serializable]
    public class UserStatusMessage : ChatUserMessage
    {
        public override string Command
        {
            get
            {
                return ChatUserMessage.Commands.STATUS_UPDATE;
            }
        }

        public UserStatusMessage()
        {
        }
    }
}
