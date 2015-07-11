using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Neo4jClient;

namespace FlyAwayPlus.Models.Relationships
{
    public class PostHasPhotoRelationship : Relationship, IRelationshipAllowingSourceNode<Post>, IRelationshipAllowingTargetNode<Photo>
    {
        public static readonly string TypeKey = "has";

        public PostHasPhotoRelationship(NodeReference targetNode)
            : base(targetNode)
        {
        }

        public PostHasPhotoRelationship(NodeReference targetNode, object data)
            : base(targetNode, data)
        {
        }

        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }

    public class PostAtPlaceRelationship : Relationship, IRelationshipAllowingSourceNode<Post>, IRelationshipAllowingTargetNode<Place>
    {
        public static readonly string TypeKey = "at";

        public PostAtPlaceRelationship(NodeReference targetNode)
            : base(targetNode)
        {
        }

        public PostAtPlaceRelationship(NodeReference targetNode, object data)
            : base(targetNode, data)
        {
        }

        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }

    public class UserHasPostRelationship : Relationship, IRelationshipAllowingSourceNode<User>,
        IRelationshipAllowingTargetNode<Post>
    {
        public static readonly string TypeKey = "has";

        public UserHasPostRelationship(NodeReference targetNode)
            : base(targetNode)
        {
        }

        public UserHasPostRelationship(NodeReference targetNode, object data)
            : base(targetNode, data)
        {
        }

        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }
}