using System;
using System.Windows;

namespace SportBetting.WPF.Prism.Shared.Models
{
    public class TicketView
    {

        public TicketView(string number, string checksum, string status, long statusId, DateTime createdAt, string currency)
        {
            Number = number;
            CheckSum = checksum;
            FullNumber = number + " " + checksum;
            Status = status;
            StatusId = statusId;
            CreatedAt = createdAt;
            Currency = currency;
        }

        public bool Hidden { get; set; }
        public string FullNumber { get; private set; }
        public string Number { get; set; }
        public string CheckSum { get; set; }
        public string Status { get; set; }
        public long StatusId { get; set; }
        public string Currency { get; set; }
        public DateTime CreatedAt { get; set; }

        public Visibility PendingApprovalVisibility
        {
            get
            {
                return StatusId == 5 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public string OpenLostWonColor
        {
            get
            {
                if (StatusId == 1) return "#ff22b613"; // state 1 == won
                else if (StatusId == 2) return "#FFFF1313"; // state 2 == lost
                else if (StatusId == 4) return "#FFFFFFFF"; // state 0 == open
                else if (StatusId == 5) return "#FF9933"; // pending for approval
                else return "#61217C"; // state 0 == canceled

            }
        }
    }
}