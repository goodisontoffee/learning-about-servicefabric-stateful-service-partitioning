using System;
using System.Runtime.Serialization;

namespace Contracts
{
    [DataContract]
    public class ServiceInteraction
    {
        [DataMember]
        public Guid PartitionId { get; set; }

        [DataMember]
        public DateTime InteractedAtUtc { get; set; }

        public ServiceInteraction()
            : this(Guid.Empty)
        {
        }

        public ServiceInteraction(Guid partitionId)
        {
            PartitionId = partitionId;
            InteractedAtUtc = DateTime.UtcNow;
        }
    }

    [DataContract]
    public class ServiceInteraction<T> : ServiceInteraction
    {
        [DataMember]
        public T Result { get; set; }

        public ServiceInteraction()
            : this(default(T))
        {
        }

        public ServiceInteraction(T result)
            : this (Guid.NewGuid(), result)
        {
        }

        public ServiceInteraction(Guid partitionId, T result)
            : base(partitionId)
        {
            Result = result;
        }
    }
}