using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor, AllowMultiple = false)]
    class ReplaceMethodAttribute : Attribute
    {
        private string MethodName { get; set; }
        private string AssemblyName { get; set; }
        private string TypeFullName { get; set; }
        public bool IsProperty { get; set; }
        public string MethodFullName { get; set; }
        public ReplaceMethodAttribute(string methodName, string assemblyName, string typeFullName)
        {
            MethodName = methodName;
            AssemblyName = assemblyName;
            TypeFullName = typeFullName;
        }
    }
}
