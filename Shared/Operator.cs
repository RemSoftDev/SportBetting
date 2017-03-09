using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class Operator : User
    {
        public int RoleID;

        public Operator(string[] permissions)
        {
           if(permissions == null || permissions.Length == 0)
                throw new ArgumentNullException("there is no permissions for this user");

            Permissions = permissions;
        }

        public bool IsSuperOperator { get; set; }

        public string[] Permissions { get; set; }
        public override void Withdrawmoney(decimal amount)
        {
            throw new NotImplementedException();
        }

        public override void Addmoney(decimal amount)
        {
            throw new NotImplementedException();
        }

        public override void Refresh()
        {
            throw new NotImplementedException();
        }

        public string Password { get; set; }

    }
}
