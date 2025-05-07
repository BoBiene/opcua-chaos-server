using Opc.Ua;
using Opc.Ua.Server;
using System.Reflection;

namespace opcua.chaos.server
{
    internal class ReflectionHelper
    {
        internal static T GetPrivateMember<T>(object owner, string memberName)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (string.IsNullOrWhiteSpace(memberName)) throw new ArgumentException("Member name cannot be null or empty.", nameof(memberName));

            // Get the type of the owner object
            Type type = owner.GetType();

            // Try to get the private field (non-public instance)
            FieldInfo field = GetFieldIncludingBase(type, memberName);
            if (field == null)
            {
                throw new InvalidOperationException($"Field '{memberName}' not found on type '{type.FullName}'.");
            }

            // Get the value and cast it
            return (T)field.GetValue(owner);
        }

        private static FieldInfo GetFieldIncludingBase(Type type, string fieldName)
        {
            while (type != null)
            {
                var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field != null)
                    return field;

                type = type.BaseType;
            }
            return null;
        }

    }

}