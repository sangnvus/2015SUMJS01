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

        public static int CountUserComment(int postID)
        {
            /*
             * Match (p:post {postID:@postID})-[r:has]->(c:comment), (u:user)-[r1:has]-(c)
                return COUNT (DISTINCT u)
             */
            try
            {
                Client.Connect();
                return Client.Cypher.Match("(p:post {postID:" + postID + "})-[r:has]->(c:comment), (u:user)-[r1:has]-(c)")
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
             * Match (p:post {postID:@postID})-[r:has]->(c:comment)
                return COUNT(DISTINCT c)
             */
            try
            {
                Client.Connect();
                return Client.Cypher.Match("(p:post {postID:" + postID + "})-[r:has]->(c:comment)")
                                .Return<int>("COUNT(distinct c)")
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
             * Match (p:post {postID:@postID})<-[r:dislike]-(u:user)
                return COUNT(DISTINCT c)
             */
            try
            {
                Client.Connect();
                return Client.Cypher.Match("(p:post {postID:" + postID + "})<-[r:dislike]-(u:user)")
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
             * Match (p:post {postID:@postID})<-[r:like]-(u:user)
                return COUNT(DISTINCT r)
             */
            try
            {
                Client.Connect();
                return Client.Cypher.Match("(p:post {postID:" + postID + "})<-[r:like]-(u:user)")
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
                return Client.Cypher.Match("(u:user {userID:" + userID + "})-[r:like]->(p:post {postID:" + postID + "})")
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
                return Client.Cypher.Match("(u:user {userID:" + userID + "})-[r:dislike]->(p:post {postID:" + postID + "})")
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
                Client.Cypher.Match("(u:user {userID:" + userID + "})-[r:like]->(p:post {postID:" + postID + "})")
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
            try { 
                Client.Connect();
                Client.Cypher.Match("(u:user {userID:" + userID + "})-[r:dislike]->(p:post {postID:" + postID + "})")
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

        public static bool InsertLike(int userID, int postID) {
            Node<User> userNode = getNodeUser(userID);

            NodeReference<Post> postRef = Client.Cypher.Match("(p:post {postID:" + postID + "})")
                                            .Return<Node<Post>>("p")
                                            .Results.Single()
                                            .Reference;
            if (userNode != null)
            {
                var userRef = userNode.Reference;
                Client.CreateRelationship(userRef, new UserLikePostRelationship(postRef, new { dateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat)}));
                return true;
            }
            return false;
        }

        public static Node<User> getNodeUser(int id)
        {
            Client.Connect();
            Node<User> userNode = Client.Cypher.Match("(u:user {userID: " + id + "})").Return<Node<User>>("u").Results.FirstOrDefault();
            return userNode;
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
                Client.CreateRelationship(userRef, new UserHasPostRelationship(postRef));
                
                Client.Cypher.Match("(u:user {userID:" + user.userID + "}), (p:photo {photoID: " + photo.photoID + "})")
                    .Create("(u)-[r:has]->(p)")
                    .ExecuteWithoutResults();

                Client.Cypher.Match("(u:user {userID:" + user.userID + "}), (p:place {placeID: " + place.placeID + "})")
                    .Create("(u)-[r:visited]->(p)")
                    .ExecuteWithoutResults();
            }
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

        public static Post FindPost(int id, User user)
        {
            /*
                 * Query:
                 * Find:
                 *     - post of current user
                 *     - post with privacy = 'public'
                 *     - post of friend with privacy = 'friend'
                 * 
                 * match(u1:user {userID:@userID})-[:friend]->(u2:user)
                   with u1, u2
                   match(p:post {postID:@postID}) 
                   where (u1-[:has]->p) or (p.privacy='friend' and u2-[:has]->p) or (p.privacy = 'public')
                   return p
                 */
            Post p = null;
            Client.Connect();
            p = Client.Cypher.Match("(u1:user {userID:" + user.userID + "})-[:friend]->(u2:user)")
                            .With("u1, u2")
                            .Match("(p:post {postID:" + id + "})")
                            .Where("(u1-[:has]->p) or (p.privacy='friend' and u2-[:has]->p) or (p.privacy = 'public')")
                            .ReturnDistinct<Post>("p")
                            .Results.Single();

            return p;
        }

        public static User SearchUser(Post post)
        {
            User user = null;
            Client.Connect();
            user = Client.Cypher.Match("(u:user)-[:has]->(p:post)")
                            .Where("p.postID=" + post.postID)
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
                 * MATCH(p:post {postID:@postID})-[:has]->(c:comment)
                    return c
                 */
            List<Comment> list = null;
            Client.Connect();
            list = Client.Cypher.Match("(p:post {postID:" + post.postID + "})-[:has]->(c:comment)")
                            .Return<Comment>("c")
                            .OrderBy("c.dateCreated")
                            .Results.ToList();
            return list;
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
            user = Client.Cypher.Match("(u:user)-[:has]->(c:comment{commentID:" + comment.commentID + "})")
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

        public static User FindUser(Post post)
        {
            /*
                 * Query:
                 * Find:
                 *     - find User has post
                 * 
                 * MATCH(u:user)-[:has]->(c:post{postID:@postID})
                    return u
                 */
            User user = null;
            Client.Connect();
            user = Client.Cypher.Match("(u:user)-[:has]->(p:post{postID:" + post.postID + "})")
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
            photo = Client.Cypher.Match("(po:post {postID:" + po.postID + "})-[:has]->(ph:photo)")
                            .Return<Photo>("ph")
                            .OrderBy("ph.photoID")
                            .Results
                            .ToList()
                            .First();
            return photo;
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
            place = Client.Cypher.Match("(po:post {postID:" + po.postID + "})-[:at]->(pl:place)")
                            .Return<Place>("pl")
                            .OrderBy("pl.placeID")
                            .Results
                            .ToList()
                            .First();
            return place;
        }

        public static List<Post> FindPostFollowing(User user)
        {
            /*
                 * Query:
                 * Find:
                 *     - post of current user
                 *     - post with privacy = 'public'
                 *     - post of friend with privacy = 'friend'
                 * 
                 * match(u1:user {userID:@userID})-[:friend]->(u2:user)
                   with u1, u2
                   match(p:post) 
                   where (u1-[:has]->p) or (p.privacy='friend' and u2-[:has]->p) or (p.privacy = 'public')
                   return p
                 */
            List<Post> listPost = null;
            Client.Connect();
            listPost = Client.Cypher.Match("(u1:user {userID:" + user.userID + "})-[:friend]->(u2:user)")
                            .With("u1, u2")
                            .Match("(p:post)")
                            .Where("(u1-[:has]->p) or (p.privacy='friend' and u2-[:has]->p) or (p.privacy = 'public')")
                            .ReturnDistinct<Post>("p")
                            .OrderByDescending("p.dateCreated")
                            .Results.ToList();

            return listPost;
        }

        public static List<Post> FindPostOfUser(User user)
        {
            /*
                 * Query:
                 * Find:
                 *     - all post of current user
                 * 
                 * match(u1:user {userID:@userID})-[:has]->(p:post)
                   return p
                 */
            List<Post> listPost = null;
            Client.Connect();
            listPost = Client.Cypher.Match("(u1:user {userID:" + user.userID + "})-[:has]->(p:post)")
                            .ReturnDistinct<Post>("p")
                            .OrderByDescending("p.dateCreated")
                            .Results.ToList();

            return listPost;
        }

        public static List<Post> FindPostOfOtherUser(User currentUser, User otherUser)
        {
            /*
                 * Query:
                 * Find:
                 *     - all post of otherUser that currentUser can view
                 * 
                 * match(u1:user {userID:@otherUserID}),(u2:user {userID:@currentUserID})
                    with u1,u2
                    match(p:post)<-[:has]-u1
                    where p.privacy='public' or (p.privacy='friend' and u1-[:friend]->u2)
                    return p
                 */
            List<Post> listPost = null;
            Client.Connect();
            listPost = Client.Cypher.Match("(u1:user {userID:" + otherUser.userID + "}), (u2:user {userID:" + currentUser.userID + "})")
                            .With("u1, u2")
                            .Match("(p:post)<-[:has]-u1")
                            .Where("p.privacy='public' or (p.privacy='friend' and u1-[:friend]->u2)")
                            .ReturnDistinct<Post>("p")
                            .OrderByDescending("p.dateCreated")
                            .Results.ToList();

            return listPost;
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
            listUser = Client.Cypher.Match("(u1:user {userID:" + user.userID + "})-[:friend]->(u2:user)")
                            .ReturnDistinct<User>("u2")
                            .Results.ToList();

            return listUser;
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
                 * match(u1:user {userID:@userID})-[:friend]->(u2:user)
                   with u1, u2
                   match(p:post) 
                   where (u1-[:has]->p) or (p.privacy='friend' and u2-[:has]->p) or (p.privacy = 'public')
                   return p
                 */
            List<Post> listPost = null;
            Client.Connect();
            listPost = Client.Cypher.Match("(u1:user {userID:" + user.userID + "})-[:friend]->(u2:user)")
                            .With("u1, u2")
                            .Match("(p:post)")
                            .Where("(u1-[:has]->p) or (p.privacy='friend' and u2-[:has]->p) or (p.privacy = 'public')")
                            .ReturnDistinct<Post>("p")
                            .OrderByDescending("p.dateCreated")
                            .Skip(skip)
                            .Limit(limit)
                            .Results.ToList();

            return listPost;
        }
    }
}