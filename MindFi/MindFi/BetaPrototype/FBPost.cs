using System;
using System.Collections;

namespace MyBackup
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
        /// ID of the object the post refers to
        /// </summary>
        // public string ObjectID { get; set; }
        /// <summary>
        /// List of comments associated to the post
        /// </summary>
        public ArrayList Comments 
	{
	    get { lock (LockObj) { return m_comments; } }
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
        /// Names of people that like the post
        /// </summary>
        public string LikesName { get; set; }
        /// <summary>
        /// IDs of people that like the post
        /// </summary>
        public string LikesID { get; set; }
        /// <summary>
        /// parent reference
        /// </summary>
        public JSONParser parent { get; set; }
        #endregion

	private ArrayList m_comments;
	private ArrayList m_likes;
	private ArrayList m_from;
	private ArrayList m_to;
	
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
	    //AddParser("from", "FBPerson", ref m_from);
	    AddParser("likes", "FBPerson", ref m_likes);
	}

	public override void Save(out string ErrorMessage)
        {
            ErrorMessage = "";
	    base.Save(out ErrorMessage);
	    if ( Saved )
	    {
		Saved = false;
		// System.Windows.Forms.MessageBox.Show("Date: " + MyPartitionDate + " ID:" + MyPartitionID);
		string ParentID = null;
		FBObject fbParent = parent as FBObject;
		if ( fbParent != null )
		{
		    ParentID = fbParent.SNID;
		}
		// System.Windows.Forms.MessageBox.Show("saving post created: " + Created + " updated: " + Updated);

	    	DBLayer.PostDataSave(MyPartitionDate, MyPartitionID, 
			FromID, FromName, ToID, ToName, Message,
			Picture, Link, Caption, Description,
			Source, Icon, Attribution, Privacy, PrivacyValue,
			m_created, m_updated,
			ActionsID, ActionsName, ApplicationID, ApplicationName,
			PostType, ParentID, CommentCount, LikesCount, 
			out Saved, out ErrorMessage);
		lock ( LockObj )
		{
                    if (m_comments != null && m_comments.Count > 0 )
                    {
			// System.Windows.Forms.MessageBox.Show("saving comments for post: " + m_comments.Count );
                        foreach (FBPost post in m_comments)
                        {
                            string error;
                            post.Save(out error);
                            ErrorMessage += error;
                        }
                    }
		}
		    // TODO: Change parser to generate likes as user list, then save corresponding relationship	
	    } else
	    {
		System.Windows.Forms.MessageBox.Show("didnt save post " + ID + " because of\n" + ErrorMessage);
	    }
        }

        protected override void AssignValue(string name, string value)
        {
            switch (name)
            {
                case "id":
                    switch ( parentName )
                    {
                        case null:
                        case "":
                            SNID = value;
                            break;
                        case "from":
                            FromID = value;
                            break;
                        case "to":
                            ToID = value;
                            break;
                        case "likes":
                            LikesID += value + ";";
                            break;
                        case "application":
                            ApplicationID = value;
                            break;
                        case "actions":
                            ActionsID += value;
                            break;
                        default:
                            lastError += "Unknown name ignored: " + parentName + name + "\n";
                            break;
                    }
                    break;
                case "name":
                    switch ( parentName )
                    {
                        case null:
                        case "":
                            Name = value;
                            break;
                        case "from":
                            FromName = value;
                            break;
                        case "to":
                            ToName = value;
                            break;
                        case "likes":
                            LikesName += value + ";";
                            break;
                        case "application":
                            ApplicationName = value;
                            break;
                        case "actions":
                            ActionsName += value;
                            break;
                        default:
                            lastError += "Unknown name ignored: " + parentName + name + "\n";
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
                    Link = value;
                    break;
                case "type":
                    PostType = value;
                    break;
/*
                case "object_id":
                    ObjectID = value;
                    break;
*/
                case "value":
                case "friends":
                    if (parentName == "privacy")
                    {
                        PrivacyValue += value + ";";
                    }
                    else
                    {
                        lastError += "Unknown name ignored: " + name + "\n";
                    }
                    break;
                case "deny":
                    if (parentName == "privacy")
                    {
                        PrivacyValue += " except " + value + ";";
                    }
                    else
                    {
                        lastError += "Unknown name ignored: " + name + "\n";
                    }
                    break;
                case "user_likes":
                case "category":
                    // User likes es simplement true cuando aparece, ignorado pues se necesita un sistema separado para obtener los "likes" hijos cuando existan
                    // this is for Posts TO a Page, for now ignoring, could add the field "ToCategory", pero primero ser�a conveniente ver la asociaci�n con los Pages que el usuario Like
                    // ignore without warning
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
			lastError += "Unknown name ignored: " + name + "\n";
		    }
		break;
/*
		case "id":
		    switch (parentName)
		    {
			case null:
			case "":
			    SNID = intValue.ToString();
			    break;
			case "from":
			    FromID = intValue.ToString();
			    break;
			case "to":
			    ToID = intValue.ToString();
			    break;
			case "likes":
			    LikesID += intValue + ";";
			    break;
			case "application":
			    ApplicationID = intValue.ToString();
			    break;
			case "actions":
			    ActionsID += intValue;
			    break;
			default:
			    lastError += "Unknown name ignored: " + parentName + name + "\n";
			    break;
                        }
                        break;
*/
		default:
                    base.AssignNumericValue(name,intValue);
                    break;
            }
        }

	#endregion

    }
}