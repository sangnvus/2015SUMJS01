using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Neo4jClient;

namespace FlyAwayPlus.Models.Relationships
{
    public class PostHasPhotoRelationship : Relationship, IRelationshipAllowingSourceNode<Post>, IRelationshipAllowingTargetNode<Photo>
    {
        public static readonly string TypeKey = "HAS";

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
        public static readonly string TypeKey = "AT";

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

    public class UserLikePostRelationship : Relationship, IRelationshipAllowingSourceNode<User>,
        IRelationshipAllowingTargetNode<Post>
    {
        public static readonly string TypeKey = "LIKE";
        public string dateCreated { get; set; }
        public int activityID { get; set; }
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

    
    public class UserCreateCommentRelationship : Relationship, IRelationshipAllowingSourceNode<User>,
        IRelationshipAllowingTargetNode<Comment>
    {
        public static readonly string TypeKey = "CREATE";
        public string dateCreated { get; set; }

        public UserCreateCommentRelationship(NodeReference targetNode)
            : base(targetNode)
        {
        }

        public UserCreateCommentRelationship(NodeReference targetNode, object data)
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
        public static readonly string TypeKey = "DISLIKE";
        public string dateCreated { get; set; }
        public int activityID { get; set; }
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

    public class UserWishPostRelationship : Relationship, IRelationshipAllowingSourceNode<User>,
        IRelationshipAllowingTargetNode<Post>
    {
        public static readonly string TypeKey = "WISH";

        public UserWishPostRelationship(NodeReference targetNode)
            : base(targetNode)
        {
        }

        public UserWishPostRelationship(NodeReference targetNode, object data)
            : base(targetNode, data)
        {
        }

        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }
}