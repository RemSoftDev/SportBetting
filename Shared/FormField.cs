using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class FormField
    {
        public string Name { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsReadonly { get; set; }
        public bool IsSearchable { get; set; }
        public bool IsHidden { get; set; }
        public string Type { get; set; }
        public int Sequence { get; set; }
        public FieldValidationRule[] ValidationRules { get; set; }
        public FieldOption[] Options { get; set; }
        public string Value { get; set; }
    }
}