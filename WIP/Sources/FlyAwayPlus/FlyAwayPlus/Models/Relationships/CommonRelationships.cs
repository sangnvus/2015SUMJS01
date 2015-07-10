using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Neo4jClient;

namespace FlyAwayPlus.Models.Relationships
{
    public class HasRelationship : Relationship, IRelationshipAllowingSourceNode<Post>, IRelationshipAllowingTargetNode<Photo>
    {
        public static readonly string TypeKey = "HAS";

        public HasRelationship(NodeReference targetNode)
            : base(targetNode)
        {
        }

        public HasRelationship(NodeReference targetNode, object data)
            : base(targetNode, data)
        {
        }

        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }

    public class AtRelationship : Relationship, IRelationshipAllowingSourceNode<Post>, IRelationshipAllowingTargetNode<Place>
    {
        public static readonly string TypeKey = "AT";

        public AtRelationship(NodeReference targetNode)
            : base(targetNode)
        {
        }

        public AtRelationship(NodeReference targetNode, object data)
            : base(targetNode, data)
        {
        }

        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }
}