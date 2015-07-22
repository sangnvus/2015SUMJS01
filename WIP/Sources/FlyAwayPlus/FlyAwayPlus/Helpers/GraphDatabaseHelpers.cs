using FlyAwayPlus.Models;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using FlyAwayPlus.Models.Relationships;

namespace FlyAwayPlus.Helpers
{
    public class GraphDatabaseHelpers
    {
        private static readonly GraphClient Client = new GraphClient(new Uri(ConfigurationManager.AppSettings["dbGraphUri"]));

        public static void InsertUser(User user)
        {
            // Auto increment Id.
            user.userID = GetGlobalIncrementId();

            Client.Connect();
            var userRef = Client.Cypher.
                        Create("(user:user {newUser})").
                        WithParam("newUser", user)
                        .Return<User>("user")
                        .Results.Single();
        }

        public static string GetNotification(int userID)
        {
            return "";
        }

        public static bool isLike(int postID, int userID)
        {
            // Auto increment Id

            Client.Connect();
            int like = Client.Cypher.Match("(u:user {userID:" + userID + "})-[r:LIKE]->(p:post {postID:" + postID + "})")
                                    .Return<int>("COUNT(r)")
                                    .Results.Single();

            return like != 0;
        }

        public static bool isDislike(int postID, int userID)
        {
            // Auto increment Id

            Client.Connect();
            int dislike = Client.Cypher.Match("(u:user {userID:" + userID + "})-[r:DISLIKE]->(p:post {postID:" + postID + "})")
                                    .Return<int>("COUNT(r)")
                                    .Results.Single();

            return dislike != 0;
        }

        public static bool isWish(int postID, int userID)
        {
            // Auto increment Id

            Client.Connect();
            int wish = Client.Cypher.Match("(u:user {userID:" + userID + "})-[r:WISH]->(p:post {postID:" + postID + "})")
                                    .Return<int>("COUNT(r)")
                                    .Results.Single();

            return wish != 0;
        }

        public static bool AddToWishList(int postID, int userID)
        {
            Client.Connect();

            NodeReference<Post> postRef = getNodePost(postID).Reference;
            Node<User> userNode = getNodeUser(userID);
            if (userNode != null)
            {
                var userRef = userNode.Reference;
                Client.CreateRelationship(userRef, new UserWishPostRelationship(postRef));
                return true;
            }
            return false;
        }

        public static bool RemoveFromWishList(int postID, int userID)
        {
            try
            {
                Client.Connect();
                Client.Cypher.Match("(u:user {userID:" + userID + "})-[r:WISH]->(p:post {postID:" + postID + "})")
                                    .Delete("r")
                                    .ExecuteWithoutResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }
        public static int CountUserComment(int postID)
        {
            /*
             * Match (p:post {postID:@postID})-[r:has]->(c:comment), (u:user)-[r1:has]-(c)
                return COUNT (DISTINCT u)
             */
            try
            {
                Client.Connect();
                return Client.Cypher.Match("(p:post {postID:" + postID + "})<-[r:COMMENTED]-(u:user)")
                                .Return<int>("COUNT(distinct u)")
                                .Results.Single();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }

        public static int CountComment(int postID)
        {
            /*
             * match(p:post {postID:@postID})-[:LATEST_COMMENT]->(c:comment)-[PREVIOUS_COMMENT*0..]->(c1:comment)
                    return Length(collect(c1)) as CommentNumber
             */
            try
            {
                Client.Connect();
                return Client.Cypher.Match("(p:post {postID:" + postID + "})-[]->(c:comment)-[PREVIOUS_COMMENT*0..]->(c1:comment)")
                                .Return<int>("Length(collect(c1)) as CommentNumber")
                                .Results.Single();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }

        public static int CountDislike(int postID)
        {
            /*
             * Match (p:post {postID:@postID})<-[r:DISLIKE]-(u:user)
                return COUNT(DISTINCT c)
             */
            try
            {
                Client.Connect();
                return Client.Cypher.Match("(p:post {postID:" + postID + "})<-[r:DISLIKE]-(u:user)")
                                .Return<int>("COUNT(distinct r)")
                                .Results.Single();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }

        public static int CountLike(int postID)
        {
            /*
             * Match (p:post {postID:@postID})<-[r:LIKE]-(u:user)
                return COUNT(DISTINCT r)
             */
            try
            {
                Client.Connect();
                return Client.Cypher.Match("(p:post {postID:" + postID + "})<-[r:LIKE]-(u:user)")
                                .Return<int>("COUNT(DISTINCT r)")
                                .Results.Single();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }

        public static int FindLike(int userID, int postID)
        {
            try
            {
                Client.Connect();
                return Client.Cypher.Match("(u:user {userID:" + userID + "})-[r:LIKE]->(p:post {postID:" + postID + "})")
                                .Return<int>("COUNT(r)")
                                .Results.Single();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }

        public static int FindDislike(int userID, int postID)
        {
            try
            {
                Client.Connect();
                return Client.Cypher.Match("(u:user {userID:" + userID + "})-[r:DISLIKE]->(p:post {postID:" + postID + "})")
                                .Return<int>("COUNT(r)")
                                .Results.Single();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }

        public static bool DeleteLike(int userID, int postID)
        {
            /**
             * Match(u:user {userID:@userID})-[r:like]->(p:post {postID:@postID})
                delete r
             */
            try
            {
                Client.Connect();
                Client.Cypher.Match("(u:user {userID:" + userID + "})-[r:LIKE]->(p:post {postID:" + postID + "})")
                                .Delete("r")
                                .ExecuteWithoutResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public static bool DeleteDislike(int userID, int postID)
        {
            /**
             * Match(u:user {userID:@userID})-[r:dislike]->(p:post {postID:@postID})
                delete r
             */
            try
            {
                Client.Connect();
                Client.Cypher.Match("(u:user {userID:" + userID + "})-[r:DISLIKE]->(p:post {postID:" + postID + "})")
                                .Delete("r")
                                .ExecuteWithoutResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public static bool InsertDislike(int userID, int postID)
        {
            Node<User> userNode = getNodeUser(userID);

            NodeReference<Post> postRef = Client.Cypher.Match("(p:post {postID:" + postID + "})")
                                            .Return<Node<Post>>("p")
                                            .Results.Single()
                                            .Reference;
            if (userNode != null)
            {
                var userRef = userNode.Reference;
                Client.CreateRelationship(userRef, new UserDislikePostRelationship(postRef, new { dateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat) }));
                return true;
            }
            return false;
        }

        public static bool InsertLike(int userID, int postID)
        {
            Node<User> userNode = getNodeUser(userID);

            NodeReference<Post> postRef = Client.Cypher.Match("(p:post {postID:" + postID + "})")
                                            .Return<Node<Post>>("p")
                                            .Results.Single()
                                            .Reference;
            if (userNode != null)
            {
                var userRef = userNode.Reference;
                Client.CreateRelationship(userRef, new UserLikePostRelationship(postRef, new { dateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat) }));
                return true;
            }
            return false;
        }

        public static Node<User> getNodeUser(int id)
        {
            Client.Connect();
            return Client.Cypher.Match("(u:user {userID: " + id + "})").Return<Node<User>>("u").Results.FirstOrDefault();
        }

        public static Node<Post> getNodePost(int id)
        {
            Client.Connect();
            return Client.Cypher.Match("(p:post {postID: " + id + "})").Return<Node<Post>>("p").Results.FirstOrDefault();
        }

        public static User GetUser(int typeId, string email)
        {
            Client.Connect();
            var listUser = Client.Cypher.Match("(u:user {typeID:" + typeId + ", email: '" + email + "'})")
                .Return<User>("u")
                .Results.ToList();
            return listUser.Count == 0 ? null : listUser.First();
        }

        public static int GetGlobalIncrementId()
        {
            Client.Connect();
            var uniqueId = Client.Cypher.Merge("(id:GlobalUniqueId)")
                            .OnCreate().Set("id.count = 1")
                            .OnMatch().Set("id.count = id.count + 1")
                            .Return<int>("id.count AS uniqueID")
                            .Results.Single();

            return uniqueId;
        }
        public static User FindUser(Comment comment)
        {
            /*
                 * Query:
                 * Find:
                 *     - find User has comment
                 * 
                 * MATCH(u:user)-[:has]->(c:comment{commentID:@commentID})
                    return u
                 */
            User user = null;
            Client.Connect();
            user = Client.Cypher.Match("(u:user)-[:CREATE]->(c:comment{commentID:" + comment.commentID + "})")
                            .Return<User>("u")
                            .Results.Single();
            return user;
        }

        public static User FindUser(int id)
        {
            /*
                 * Query:
                 * Find:
                 *     - find User has id
                 * 
                 * MATCH(u:user {userID:@userID})
                    return u
                 */
            User user = null;
            try
            {
                Client.Connect();
                user = Client.Cypher.Match("(u:user {userID:" + id + "})")
                                .Return<User>("u")
                                .Results.Single();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                user = null;
            }
            return user;
        }

        public static List<User> FindFriend(User user)
        {

            /*
                 * Query:
                 * Find:
                 *     - all friend of current user
                 * 
                 * match(u1:user{userID:@userID})-[m:friend]->(u2:user)
                    return u2;
                 */
            List<User> listUser = null;
            Client.Connect();
            listUser = Client.Cypher.Match("(u1:user {userID:" + user.userID + "})-[:FRIEND]->(u2:user)")
                            .ReturnDistinct<User>("u2")
                            .Results.ToList();

            return listUser;
        }
        public static List<Post> FindLimitWishlist(User user, int skip, int limit)
        {
            /*
                 * Query:
                 * Find:
                 *     - wishlist
                 * 
                 * match(u1:user {userID:@userID})-[:wish]->(p:post)
                   return p
                   orderby p.dateCreated
                 */
            List<Post> listPost = null;
            Client.Connect();
            listPost = Client.Cypher.Match("(u1:user {userID:" + user.userID + "})-[:WISH]->(p:post)")
                            .ReturnDistinct<Post>("p")
                            .OrderByDescending("p.dateCreated")
                            .Skip(skip)
                            .Limit(limit)
                            .Results.ToList();

            return listPost;
        }
        public static void InsertPost(User user, Post post, Photo photo, Place place)
        {
            // Auto increment Id.
            post.postID = GetGlobalIncrementId();
            photo.photoID = GetGlobalIncrementId();
            place.placeID = GetGlobalIncrementId();

            Client.Connect();

            NodeReference<Post> postRef = Client.Cypher.Create("(p:post {newPost})")
                                            .WithParam("newPost", post)
                                            .Return<Node<Post>>("p")
                                            .Results.Single()
                                            .Reference;

            NodeReference<Photo> photoRef = Client.Cypher.Create("(p:photo {newPhoto})")
                                            .WithParam("newPhoto", photo)
                                            .Return<Node<Photo>>("p")
                                            .Results.Single()
                                            .Reference;

            NodeReference<Place> placeRef = Client.Cypher.Create("(p:place {newPlace})")
                                            .WithParam("newPlace", place)
                                            .Return<Node<Place>>("p")
                                            .Results.Single()
                                            .Reference;

            Node<User> userNode = getNodeUser(user.userID);
            if (userNode != null)
            {
                var userRef = userNode.Reference;

                Client.CreateRelationship(postRef, new PostHasPhotoRelationship(photoRef));
                Client.CreateRelationship(postRef, new PostAtPlaceRelationship(placeRef));

                Client.Cypher.Match("(u:user {userID:" + user.userID + "}), (p:photo {photoID: " + photo.photoID + "})")
                    .Create("(u)-[r:HAS]->(p)")
                    .ExecuteWithoutResults();

                Client.Cypher.Match("(u:user {userID:" + user.userID + "}), (p:place {placeID: " + place.placeID + "})")
                    .Create("(u)-[r:VISITED]->(p)")
                    .ExecuteWithoutResults();

                //Client.CreateRelationship(userRef, new UserHasPostRelationship(postRef));
                /*
                 * Check User has post:
                 *  if yes do the following step:
                 *      1. DELETE the LATEST_POST relationship from user to oldPost
                 *      2. CREATE LATEST_POST relationship from user to newPost
                 *      3. CREATE PREV_POST relationship from newPost to oldPost
                 *      
                 *  else do the following step:
                 *      1. CREATE LATEST_POST relationship from user to newPost
                 */
                int oldPost = Client.Cypher.Match("(u:user{userID:" + user.userID + "})-[:LATEST_POST]->(p:post)")
                                    .Return<int>("COUNT (p)")
                                    .Results.Single();

                if (oldPost == 0)
                {
                    // CREATE New LATEST_POST
                    Client.Cypher.Match("(u:user{userID:" + user.userID + "}), (p1:post{postID:" + post.postID + "})")
                                    .Create("(u)-[:LATEST_POST]->(p1)")
                                    .ExecuteWithoutResults();
                }
                else
                {
                    Client.Cypher.Match("(u:user{userID:" + user.userID + "})-[r:LATEST_POST]->(p:post), (p1:post{postID:" + post.postID + "})")
                                    .Delete("r")
                                    .Create("(u)-[:LATEST_POST]->(p1)")
                                    .Create("(p1)-[:PREV_POST]->(p)")
                                    .ExecuteWithoutResults();
                }
            }
        }
        public static User SearchUser(Post post)
        {
            /**
             * match (p:post{postID:1003})-[:PREV_POST*0..]-(p1:post)-[:LATEST_POST]-(u:user)
                return u
             */
            User user = null;
            Client.Connect();
            user = Client.Cypher.Match("(p:post{postID:" + post.postID + "})-[:PREV_POST*0..]-(p1:post)-[:LATEST_POST]-(u:user)")
                            .ReturnDistinct<User>("u")
                            .Results.Single();
            return user;
        }

        public static List<Comment> FindComment(Post post)
        {
            /*
                 * Query:
                 * Find:
                 *     - find comment of post
                 * 
                 * match (p:post{postID:@postID})-[:LATEST_COMMENT]-(c:comment)-[:PREV_COMMENT*0..]-(c1:comment)
                    return c1
                 */
            List<Comment> list = null;
            Client.Connect();
            list = Client.Cypher.Match("(p:post{postID:" + post.postID + "})-[:LATEST_COMMENT]-(c:comment)-[:PREV_COMMENT*0..]-(c1:comment)")
                            .Return<Comment>("c1")
                //.OrderBy("c.dateCreated")
                            .Results.ToList();
            return list;
        }
        public static List<Post> FindPostOfUser(User user)
        {
            /*
                 * Query:
                 * Find:
                 *     - all post of current user
                 * 
                 * match (u:user{userID:@userID})-[:LATEST_POST]-(p:post)-[:PREV_POST*0..]-(p1:post)
                    return p1
                 */
            List<Post> listPost = null;
            Client.Connect();
            listPost = Client.Cypher.Match("(u:user {userID:" + user.userID + "})-[:LATEST_POST]-(p:post)-[:PREV_POST*0..]-(p1:post)")
                            .ReturnDistinct<Post>("p1")
                //.OrderByDescending("p.dateCreated")
                            .Results.ToList();

            return listPost;
        }
        public static Photo FindPhoto(Post po)
        {

            /*
                 * Query:
                 * Find:
                 *     - Find photo of post
                 * 
                 * match (po:post {postID:@postID})-[:has]->(ph:photo)
                    return ph
                 */

            Photo photo = null;
            Client.Connect();
            photo = Client.Cypher.Match("(po:post {postID:" + po.postID + "})-[:HAS]->(ph:photo)")
                            .Return<Photo>("ph")
                            .OrderBy("ph.photoID")
                            .Results
                            .ToList()
                            .First();
            return photo;
        }
        public static User FindUser(Post post)
        {
            /*
                 * Query:
                 * Find:
                 *     - find User has post
                 * 
                 * match (p:post{postID:@postID})-[:PREV_POST*0..]-(p1:post)-[:LATEST_POST]-(u:user)
                    return u
                 */
            User user = null;
            Client.Connect();
            user = Client.Cypher.Match("(p:post{postID:" + post.postID + "})-[:PREV_POST*0..]-(p1:post)-[:LATEST_POST]-(u:user)")
                            .Return<User>("u")
                            .Results.Single();
            return user;
        }
        public static List<Post> SearchAllPost()
        {

            /*
                 * Query:
                 * Find:
                 *     - Search all post with privacy public
                 * 
                 * match (p:post)
                    where p.privacy='public'
                    return p
                 */

            List<Post> listPost = null;
            Client.Connect();
            listPost = Client.Cypher.Match("(p:post)")
                            .Where("p.privacy='public'")
                            .Return<Post>("p")
                            .OrderByDescending("p.dateCreated")
                            .Results.ToList();
            return listPost;
        }
        public static Place FindPlace(Post po)
        {

            /*
                 * Query:
                 * Find:
                 *     - Find Place of post
                 * 
                 * match (po:post {postID:@postID})-[:at]->(pl:place)
                    return pl
                 */

            Place place = null;
            Client.Connect();
            place = Client.Cypher.Match("(po:post {postID:" + po.postID + "})-[:AT]->(pl:place)")
                            .Return<Place>("pl")
                            .OrderBy("pl.placeID")
                            .Results
                            .ToList()
                            .First();
            return place;
        }
        public static List<Post> FindPostOfOtherUser(User currentUser, User otherUser)
        {
            /*
                 * Query:
                 * Find:
                 *     - all post of otherUser that currentUser can view
                 * 
                 * match (u1:user{userID:@otherUserID})-[:LATEST_POST]-(p:post)-[:PREV_POST*0..]-(p1:post), (u2:user{userID:@userID})
                    where p1.privacy = 'public' or (p1.privacy = 'friend' and u1-[:FRIEND]-u2)
                    return p1
                 */
            List<Post> listPost = null;
            Client.Connect();
            listPost = Client.Cypher.Match("(u1:user {userID:" + otherUser.userID + "})-[:LATEST_POST]-(p:post)-[:PREV_POST*0..]-(p1:post), (u2:user {userID:" + currentUser.userID + "})")
                            .Where("p1.privacy = 'public' or (p1.privacy = 'friend' and u1-[:FRIEND]-u2)")
                            .ReturnDistinct<Post>("p1")
                //.OrderByDescending("p.dateCreated")
                            .Results.ToList();

            return listPost;
        }
        public static List<Post> FindLimitPostFollowing(User user, int skip, int limit)
        {
            /*
                 * Query:
                 * Find:
                 *     - post of current user
                 *     - post with privacy = 'public'
                 *     - post of friend with privacy = 'friend'
                 * 
                 * optional match (u1:user{userID:@userID})-[:LATEST_POST]-(a:post)-[:PREV_POST*0..]-(p1:post)
                    optional match (u1)-[:FRIEND]-(u2:user)-[:LATEST_POST]-(b:post)-[:PREV_POST*0..]-(p2:post)
                    with collect(distinct p1) as list1, collect(distinct p2) as list2
                    match (p3:post)
                    where p3.privacy = 'public' or (p3 in list1) or (p3 in list2 and p3.privacy='friend')
                    return distinct p3
                 */
            List<Post> listPost = null;
            Client.Connect();
            listPost = Client.Cypher.OptionalMatch("(u1:user{userID:" + user.userID + "})-[:LATEST_POST]-(a:post)-[:PREV_POST*0..]-(p1:post)")
                            .OptionalMatch("(u1)-[:FRIEND]-(u2:user)-[:LATEST_POST]-(b:post)-[:PREV_POST*0..]-(p2:post)")
                            .With("collect(distinct p1) as list1, collect(distinct p2) as list2")
                            .Match("(p3:post)")
                            .Where("p3.privacy = 'public' or (p3 in list1) or (p3 in list2 and p3.privacy='friend')")
                            .ReturnDistinct<Post>("p3")
                            .OrderByDescending("p3.dateCreated")
                            .Skip(skip)
                            .Limit(limit)
                            .Results.ToList();

            return listPost;
        }
        public static Post FindPost(int id, User user)
        {
            /*
                 * Query:
                 * Find:
                 *     - post of current user
                 *     - post with privacy = 'public'
                 *     - post of friend with privacy = 'friend'
                 * 
                 * MATCH (u:user { userID:@userID }),(p:post { postID: @postID}), c = shortestPath((u)-[:LATEST_POST|:PREV_POST|:FRIEND*..]-(p))
                    RETURN COUNT(c)
                 */
            Post p = null;
            Client.Connect();
            p = Client.Cypher.Match("(p:post {postID:" + id + "})")
                            .ReturnDistinct<Post>("p")
                            .Results.SingleOrDefault();
            if (p == null || !p.privacy.Equals("public"))
            {
                int path = Client.Cypher.Match("(u:user {userID:" + user.userID + "}),(p:post { postID: " + id + "}), c = shortestPath((u)-[:LATEST_POST|:PREV_POST|:FRIEND*..]-(p))")
                            .ReturnDistinct<int>("COUNT(c)")
                            .Results.Single();
                if (path != 0)
                {
                    return p;
                }
                return null;
            }
            else
            {
                return p;
            }
        }
        public static List<Post> SearchLimitPost(int skip, int limit)
        {

            /*
                 * Query:
                 * Find:
                 *     - Search limit post with privacy public
                 * 
                 * match (p:post)
                    where p.privacy='public'
                    return p
                 */

            List<Post> listPost = null;
            Client.Connect();
            listPost = Client.Cypher.Match("(p:post)")
                            .Where("p.privacy='public'")
                            .Return<Post>("p")
                            .OrderByDescending("p.dateCreated")
                            .Skip(skip)
                            .Limit(limit)
                            .Results.ToList();
            return listPost;
        }
        public static bool InsertComment(int postID, Comment comment, int userID)
        {
            // Auto increment Id
            comment.commentID = GetGlobalIncrementId();

            Client.Connect();
            NodeReference<Comment> comRef = Client.Cypher.Create("(c:comment {newComment})").
                        WithParam("newComment", comment)
                        .Return<Node<Comment>>("c")
                        .Results.Single().Reference;

            Node<User> userNode = getNodeUser(userID);
            if (userNode != null)
            {
                var userRef = userNode.Reference;
                Client.CreateRelationship(userRef, new UserCreateCommentRelationship(comRef));

                //Client.CreateRelationship(postRef, new PostHasCommentRelationship(comRef));
                /*
                 * Check post has comment:
                 *  if yes do the following step:
                 *      1. DELETE the LATEST_COMMENT relationship from user to oldPost
                 *      2. CREATE LATEST_COMMENT relationship from user to newPost
                 *      3. CREATE PREV_COMMENT relationship from newPost to oldPost
                 *      
                 *  else do the following step:
                 *      1. CREATE LATEST_COMMENT relationship from user to newPost
                 */
                int oldComment = Client.Cypher.Match("(p:post{postID:" + postID + "})-[:LATEST_COMMENT]->(c:comment)")
                                    .Return<int>("COUNT (c)")
                                    .Results.Single();

                if (oldComment == 0)
                {
                    // CREATE New LATEST_POST
                    Client.Cypher.Match("(p:post{postID:" + postID + "}), (c:comment{commentID:" + comment.commentID + "})")
                                    .Create("(p)-[:LATEST_POST]->(c)")
                                    .ExecuteWithoutResults();
                }
                else
                {
                    Client.Cypher.Match("(p:post{postID:" + postID + "})-[r:LATEST_COMMENT]->(c:comment), (c1:comment{commentID:" + comment.commentID + "})")
                                    .Delete("r")
                                    .Create("(p)-[:LATEST_POST]->(c1)")
                                    .Create("(c1)-[:PREV_POST]->(c)")
                                    .ExecuteWithoutResults();
                }
                return true;
            }
            return false;
        }
    }
}