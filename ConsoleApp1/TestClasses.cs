using MessagePack;
using ProtoBuf;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
namespace ConsoleApp1
{

    /// <summary>
    /// A class that has several private members, no parameterless c'tor and a
    /// am implicit property that does not need any serialization. Morover it can have a reference to a
    /// ClassB object. 
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    [MessagePackObject(AllowPrivate = true)]
    [ProtoContract(SkipConstructor = true)]
    public partial class ClassA : IEquatable<ClassA>
    {
        [Key(0)]
        [ProtoMember(1)]
        [DataMember]
        private int m_value;

        [Key(1)]
        [ProtoMember(2)]
        [DataMember]
        private ClassB? m_classB = null;

        [Key(2)]
        [ProtoMember(3)]
        [DataMember]
        private ClassC? m_classC = null;

        // note: no parameterless c'tor needed

        [IgnoreMember]
        [ProtoIgnore]
        [IgnoreDataMember]
        public int ObjectIdentifier { get { return RuntimeHelpers.GetHashCode(this); } }

        public ClassA(int v)
        {
            m_value = v;
            m_classB = new ClassB();
            m_classB.SetValue(v - 5);
        }

        public void SetValue(int v)
        {
            m_value = v;
            m_classB = new ClassB();
            m_classB.SetValue(v - 5);
        }

        public void SetC(ClassC c)
        {
            c.classA = this;
            m_classC = c;
        }

        public override bool Equals(object? obj)
        {
            if(obj == null ||  obj.GetType() != typeof(ClassA))
                return false;

            return Equals((ClassA)obj);
        }

        public bool Equals(ClassA? other)
        {
            if (other == null)
                return false;

            if (m_value != other.m_value) 
                return false;

            if (m_classB == null && other.m_classB != null)
                return false;

            if (m_classB != null && other.m_classB == null)
                return false;

            if (m_classB != null && other.m_classB != null && m_classB.Equals(other.m_classB) == false)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return 0;
        }  
    }

    /// <summary>
    /// A class that has a dictionary as a member. 
    /// </summary>
    [Serializable]
    [MessagePackObject(AllowPrivate = true)]
    [ProtoContract]
    public partial class ClassB: IEquatable<ClassB>
    {
        [Key(0)]
        [ProtoMember(1)]
        private int m_value;

        [Key(1)]
        [ProtoMember(2)]
        private Dictionary<int, int> m_counters = new Dictionary<int, int>();

        [IgnoreMember]
        [ProtoIgnore]
        [IgnoreDataMember]
        public int ObjectIdentifier { get { return RuntimeHelpers.GetHashCode(this); } }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != typeof(ClassB))
                return false;

            return Equals((ClassB)obj);
        }

        public bool Equals(ClassB? other)
        {
            if (other == null)
                return false;

            if (m_value != other.m_value)
                return false;

            if(m_counters.SequenceEqual(other.m_counters) == false)
            {
                return false;
            }

            return true;
        }

        public void SetValue(int v)
        {
            m_value = v;

            if(m_counters.ContainsKey(v) == false)
            {
                m_counters[v] = 0;  
            }

            m_counters[v]++;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }

    /// <summary>
    /// A class that contains a reference to CLass as a simple property. Together with class a there can be a circular reference.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    [ProtoContract]
    public class ClassC : IEquatable<ClassC>
    {
        [Key(0)]
        [ProtoMember(1)]
        public ClassA? classA { get; set; } = null;

        [IgnoreMember]
        [ProtoIgnore]
        [IgnoreDataMember]
        public int ObjectIdentifier { get { return RuntimeHelpers.GetHashCode(this); } }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != typeof(ClassC))
                return false;

            return Equals((ClassC)obj);
        }

        public bool Equals(ClassC? other)
        {
            if (other == null)
                return false;

            if (this.classA == null && other.classA == null)
            {
                return true;
            }

            if (this.classA == null && other.classA != null)
            {
                return false;
            }

            if (this.classA != null && other.classA == null)
            {
                return false;
            }

            if (classA != null)
                return classA.Equals(other.classA);

            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }

}
