using System;
using System.Collections;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Class that represents a Facebook post
    /// It contains the parsing for importing data from a JSON response representing a Post object taken using the FB Graph API 
    ///     https://developers.facebook.com/docs/reference/api/post/
    /// It contains the logic to save to the MyBackup database (Posts table)
    /// </summary>
    public class FBPost : FBObject
    {
        #region "Properties"

        #region "Standard FB Properties"
        /// <summary>
        /// Social network ID of the user who posts
        /// </summary>
        public string FromID { get; set; }
        /// <summary>
        /// Name of the user who posts
        /// </summary>
        public string FromName { get; set; }
        /// <summary>
        /// Social network ID of the user to which the post is sent
        /// </summary>
        public string ToID { get; set; }
        /// <summary>
        /// Name of the user to which the post is sent
        /// </summary>
        public string ToName { get; set; }
        /// <summary>
        /// Main content of the post
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Link to picture
        /// </summary>
        public string Picture { get; set; }
        /// <summary>
        /// Link to which the post refers to
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// Caption that appears next to the link
        /// </summary>
        public string Caption { get; set; }
        /// <summary>
        /// Description of the post
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// URL to embed
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// Link to icon picture
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// Which application was used to create this post
        /// </summary>
        public string Attribution { get; set; }
        /// <summary>
        /// Privacy data
        /// </summary>
        public string Privacy { get; set; }
        /// <summary>
        /// Privacy data
        /// </summary>
        public string PrivacyValue { get; set; }
        /// <summary>
        /// ID of the actions allowed
        /// </summary>
        public string ActionsID { get; set; }
        /// <summary>
        /// Name of the actions allowed
        /// </summary>
        public string ActionsName { get; set; }
        /// <summary>
        /// ID of the application who posts
        /// </summary>
        public string ApplicationID { get; set; }
        /// <summary>
        /// Name of the application who posts
        /// </summary>
        public string ApplicationName { get; set; }
        /// <summary>
        /// Type of the post, e.g. link, photo, ...
        /// </summary>
        public string PostType { get; set; }
        /// <summary>
        /// Post category
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// Likes
        /// </summary>
        public bool HasLikes { get; set; }
        /// <summary>
        /// Verbalized story
        /// </summary>
        public string Story { get; set; }
        /// <summary>
        /// Place for check ins: ID
        /// </summary>
        public string PlaceSNID { get; set; }
        /// <summary>
        /// Place for check ins: Name
        /// </summary>
        public string PlaceName { get; set; }
        /// <summary>
        /// Tags associated to this story
        /// </summary>
        public ArrayList StoryTags
        {
            get { lock (LockObj) { return m_stags; } }
        }
        /// <summary>
        /// List of comments associated to the post
        /// </summary>
        public ArrayList Comments
        {
            get { lock (LockObj) { return m_comments; } }
        }
        /// <summary>
        /// List of comments associated to the post
        /// </summary>
        public ArrayList To
        {
            get { lock (LockObj) { return m_to; } }
        }
        /// <summary>
        /// count of comments associated to the post
        /// </summary>
        public int? CommentCount { get; set; }
        /// <summary>
        /// count of likes associated to the post
        /// </summary>
        public int? LikesCount { get; set; }
        /// <summary>
        /// parent reference
        /// </summary>
        public JSONParser parent { get; set; }
        #endregion

        private ArrayList m_comments;
        private ArrayList m_likes;
        private ArrayList m_to;
        private ArrayList m_stags;
        private ArrayList m_mtags;
        private ArrayList m_wtags;
        private ArrayList m_actions;
        private ArrayList m_properties;

        #endregion

        #region "Methods"
        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        public FBPost(string response)
            : base(response)
        {
            PostInit();
        }

        /// <summary>
        /// Default constructor, based on a scanner already in progress. For example, it is used when parsing a user Wall, which contains Post elements
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        public FBPost(JSONScanner scanner)
            : base(scanner)
        {
            PostInit();
        }

        /// <summary>
        /// Default constructor, based on a scanner already in progress. For example, it is used when parsing a user Wall, which contains Post elements
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        /// <param name="Parent">Post which contains this post / comment</param>
        public FBPost(JSONScanner scanner, JSONParser Parent)
            : base(scanner)
        {
            parent = Parent;
            PostInit();
        }

        private void PostInit()
        {
            MyDataTable = "PostData";
            AddParser("comments", "FBPost", ref m_comments);
            AddParser("to", "FBPerson", ref m_to);
            AddParser("likes", "FBPerson", ref m_likes);
            AddParser("story_tags", "FBStoryTag", ref m_stags);
            AddParser("message_tags", "FBStoryTag", ref m_mtags);
            AddParser("with_tags", "FBStoryTag", ref m_wtags);
            AddParser("actions", "FBPost", ref m_actions);
            AddParser("properties", "FBPost", ref m_properties);
            // default value for creation when it is not downloaded
            m_created = DateTime.Now;
        }

        public override void Save(out string ErrorMessage)
        {
            ErrorMessage = "";
            base.Save(out ErrorMessage);
            if (Saved)
            {
                Saved = false;
                string ParentID = null;
                FBObject fbParent = parent as FBObject;
                if (fbParent != null)
                {
                    ParentID = fbParent.SNID;
                }
                if (PostType == null || PostType == "")
                {
                    PostType = "comment";
                }

                if (m_to != null && m_to.Count > 0 )
                {
                    ToName = "";
                    foreach (FBPerson dest in m_to)
                    {
                        ToName += dest.Name + "; ";
                    }
                }

                // TODO: Save Place and relationship
                DBLayer.PostDataSave(MyPartitionDate, MyPartitionID,
                    FromID, FromName, ToID, ToName, Message,
                    Picture, Link, Caption, Description, Story,
                    Source, Icon, Attribution, Privacy, PrivacyValue,
                    m_created, m_updated,
                    ActionsID, ActionsName, ApplicationID, ApplicationName,
                    PostType, ParentID, CommentCount, LikesCount, Parsed,
                    out Saved, out ErrorMessage);
                lock (LockObj)
                {
                    if (m_comments != null && m_comments.Count > 0)
                    {
                        // System.Windows.Forms.MessageBox.Show("saving comments for post: " + m_comments.Count );
                        foreach (FBPost post in m_comments)
                        {
                            string error;
                            post.Save(out error);
                            ErrorMessage += error;
                        }
                    }
                    if (m_likes != null && m_likes.Count > 0)
                    {
                        // TODO: modularize
                        foreach (FBPerson who in m_likes)
                        {
                            //System.Windows.Forms.MessageBox.Show ( "Saving " + who.SNID + " who likes " + parentSNID );
                            string error;
                            // save likes relationship
                            DBLayer.ActionDataSave(who.SNID, SNID, Verb.LIKE, out Saved, out error);
                            ErrorMessage += error;
                            who.Save(out error);
                            ErrorMessage += error;
                        }
                    }
                    if (m_to != null && m_to.Count > 0)
                    {
                        // TODO: modularize
                        foreach (FBPerson dest in m_to)
                        {
                            dest.Distance = 2;
                            string error;
                            // save likes relationship
                            DBLayer.ActionDataSave(dest.SNID, SNID, Verb.SENTTO, out Saved, out error);
                            ErrorMessage += error;
                            dest.Save(out error);
                            ErrorMessage += error;
                        }
                    }
                    if ( m_wtags != null && m_wtags.Count > 0 )
                    {
                        string error;
                        SaveList(m_wtags, Verb.TAG, out error);
                        ErrorMessage += ErrorMessage;
                    }
                    if (m_mtags != null && m_mtags.Count > 0)
                    {
                        string error;
                        SaveList(m_mtags, Verb.TAG, out error);
                        ErrorMessage += ErrorMessage;
                    }
                    if (m_stags != null && m_stags.Count > 0)
                    {
                        string error;
                        SaveList(m_wtags, Verb.TAG, out error);
                        ErrorMessage += ErrorMessage;
                    }
                }
                // TODO: Change parser to generate likes as user list, then save corresponding relationship	
            }
            else
            {
                //System.Windows.Forms.MessageBox.Show("didnt save post " + ID + " because of\n" + ErrorMessage);
            }
        }

        private void SaveList(ArrayList list, int aVerb, out string errorMessage)
        {
            errorMessage = "";
            foreach (FBStoryTag who in list)
            {
                string error;
                // save likes relationship
                DBLayer.ActionDataSave(who.SNID, SNID, aVerb, out Saved, out error);
                errorMessage += error;
                who.Save(out error);
                errorMessage += error;
            }

        }

        protected override void AssignValue(string name, string value)
        {
            switch (name)
            {
                case "id":
                    switch ( parentName )
                    {
                        case "from":
                            FromID = value;
                            break;
                        case "place":
                            PlaceSNID = value;
                            break;
                        case "to":
                            ToID = value;
                            break;
                            /*
                    case "likes":
                        LikesID += value + ";";
                        break;
                         */
                        case "application":
                            ApplicationID = value;
                            break;
                        case "actions":
                            ActionsID += value;
                            break;
                        default:
                            base.AssignValue(name,value);
                            break;
                    }
                    break;
                case "name":
                    switch ( parentName )
                    {
                        case "from":
                            FromName = value;
                            break;
                        case "place":
                            PlaceName = value;
                            break;
                        case "to":
                            ToName = value;
                            break;
                        /*
                    case "likes":
                        LikesName += value + ";";
                        break;
                         */
                        case "application":
                            ApplicationName = value;
                            break;
                        case "actions":
                            ActionsName += value;
                            break;
                        default:
                            base.AssignValue(name, value);
                            break;
                    }
                    break;
                case "description":
                    switch ( parentName )
                    {
                        case null:
                        case "":
                            Description = value;
                            break;
                        case "privacy":
                            Privacy += value;
                            break;
                    }
                    break;
                case "message":
                    Message = value;
                    break;
                case "caption":
                    Caption = value;
                    break;
                case "attribution":
                case "application": // note: it seems to be equivalent but a documentation error
                    Attribution = value;
                    break;
                case "source":
                    Source = value;
                    break;
                case "picture":
                    Picture = value;
                    break;
                case "icon":
                    Icon = value;
                    break;
                case "link":
                case "href": // for properties, as it seems to provide the same function
                    Link = value;
                    break;
                case "type":
                    PostType = value;
                    break;
                case "category":
                    Category = value;
                    break;
                case "story":
                    Story = value;
                    break;
                case "value":
                case "friends":
                    if (parentName == "privacy")
                    {
                        PrivacyValue += value + ";";
                    }
                    else
                    {
                        base.AssignValue(name,value);
                    }
                    break;
                case "deny":
                    if (parentName == "privacy")
                    {
                        PrivacyValue += " except " + value + ";";
                    }
                    else
                    {
                        base.AssignValue(name, value);
                    }
                    break;
                case "user_likes":
                    // TODO: it may be useful to optimize likes...
                    // User likes es simplement true cuando aparece, previamente ignorado pues se necesita un sistema separado para obtener los "likes" hijos cuando existan
                    HasLikes = true;
                    break;
                default:
                    base.AssignValue(name,value);
                    break;
            }
        }

        protected override void AssignNumericValue(string name, int intValue)
        {
            switch (name)
            {
                case "likes":
                    LikesCount = intValue;
                    break;
                case "count":
                    if (parentName == "comments")
                    {
                        CommentCount = intValue;
                    }
                    else
                    {
                        base.AssignNumericValue(name, intValue);
                    }
                    break;
                default:
                    base.AssignNumericValue(name, intValue);
                    break;
            }
        }

	#endregion

    }
}