using System;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class BaseTypeOfAttribute : Attribute
    {
        private string AssemblyName { get; set; }
        private string TypeFullName { get; set; }
        public BaseTypeOfAttribute(string typeFullName,string assemblyName)
        {
            TypeFullName = typeFullName;
            AssemblyName = assemblyName;
        }
    }
}
