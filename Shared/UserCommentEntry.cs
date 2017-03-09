using System;

namespace SharedInterfaces
{
    [System.Serializable]
    public class UserCommentEntry
    {
        public string Comment { get; set; }

        public int CommentId { get; set; }

        public DateTime EditTime { get; set; }

        public long OperatorId { get; set; }

        public string OperatorName { get; set; }
    }
}