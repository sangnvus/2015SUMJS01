using FlyAwayPlus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace FlyAwayPlus.Helpers
{
    public class UserHelpers
    {
        private const string USER = "user";
        public static User getCurrentUser(HttpSessionStateBase session)
        {

            var user_session = session[USER];
            User user = null;

            if (user_session == null || user_session.ToString().Length == 0)
            {
                return null;
            }
            else
            {
                user = (User)user_session;
            }

            return user;
        }
        public static void setCurrentUser(HttpSessionStateBase session, User user)
        {
            session[USER] = user;
        }
    }
}