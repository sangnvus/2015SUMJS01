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

    public class UserLikePostRelationship : Relationship, IRelationshipAllowingSourceNode<User>,
        IRelationshipAllowingTargetNode<Post>
    {
        public static readonly string TypeKey = "like";
        public string dateCreated { get; set; }

        public UserLikePostRelationship(NodeReference targetNode)
            : base(targetNode)
        {
        }

        public UserLikePostRelationship(NodeReference targetNode, object data)
            : base(targetNode, data)
        {
        }

        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }

    public class PostHasCommentRelationship : Relationship, IRelationshipAllowingSourceNode<Post>,
        IRelationshipAllowingTargetNode<Comment>
    {
        public static readonly string TypeKey = "has";
        public string dateCreated { get; set; }

        public PostHasCommentRelationship(NodeReference targetNode)
            : base(targetNode)
        {
        }

        public PostHasCommentRelationship(NodeReference targetNode, object data)
            : base(targetNode, data)
        {
        }

        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }
    public class UserHasCommentRelationship : Relationship, IRelationshipAllowingSourceNode<User>,
        IRelationshipAllowingTargetNode<Comment>
    {
        public static readonly string TypeKey = "has";
        public string dateCreated { get; set; }

        public UserHasCommentRelationship(NodeReference targetNode)
            : base(targetNode)
        {
        }

        public UserHasCommentRelationship(NodeReference targetNode, object data)
            : base(targetNode, data)
        {
        }

        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }

    public class UserDislikePostRelationship : Relationship, IRelationshipAllowingSourceNode<User>,
        IRelationshipAllowingTargetNode<Post>
    {
        public static readonly string TypeKey = "dislike";
        public string dateCreated { get; set; }

        public UserDislikePostRelationship(NodeReference targetNode)
            : base(targetNode)
        {
        }

        public UserDislikePostRelationship(NodeReference targetNode, object data)
            : base(targetNode, data)
        {
        }

        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }
}