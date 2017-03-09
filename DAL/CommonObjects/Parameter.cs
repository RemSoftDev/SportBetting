using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SportRadar.DAL.CommonObjects
{
    /// <summary>
    /// Named paramenter holder
    /// </summary>
    public struct Parameter
    {
        public readonly string ParamName;
        public readonly object ParamValue;
        public readonly ParameterDirection ParamDirection;

        public Parameter(string name, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            ParamName = name;
            ParamValue = value;
            ParamDirection = direction;
        }

        public string toString()
        {
            return "name = " + ParamName + " val = " + ParamValue + " direction " + ParamDirection;
        }

    }

}
