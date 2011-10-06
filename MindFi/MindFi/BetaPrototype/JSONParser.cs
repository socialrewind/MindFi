using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace MyBackup
{
    /// <summary>
    /// Class that abstracts a JSON parser, base for most parser objects (formerly FBParser)
    /// It contains the general parsing objects, but the logic is dependent of the child type
    /// It also contains logic to save generic requests to the database for debug purposes
    /// </summary>

    public abstract class JSONParser
    {
        #region "Properties"
        /// <summary>
        /// ID for the object, associated to Entities table
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Display name for the object
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Reference to last error while parsing / loading / saving data for the person
        /// </summary>
        public string lastError { get; set; }
        /// <summary>
        /// JSON response parsed to get the data
        /// </summary>
        public string OriginalResponse { get; set; }
        /// <summary>
        /// Flag to indicate if the OriginalResponse has been analyzed and object initialized with corresponding values
        /// </summary>
        public bool Parsed { get; set; }
        /// <summary>
        /// Flag to indicate if the object has been saved to the database
        /// </summary>
        public bool Saved;
        /// <summary>
        /// Relationship distance with the object. 0=self, 1=friend, 2=friends of friends
        /// </summary>
        public int Distance { get; set; }
        #endregion

        #region "Internal Variables"
        protected JSONScanner temp;
        protected string token;
        protected JSONScanner.JSONScannerTokens tokenId;
        protected int nestingLevel;
        protected int parserState;
        protected string parentName;
        protected bool ConstructOriginalResponse;
        protected ArrayList ChildrenParsers;
        protected long? parentID;
        protected string parentSNID;
        #endregion

        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        /// <param name="distance">indicates if the person is the user, a friend or other</param>
        public JSONParser(string response)
        {
            OriginalResponse = response;
            temp = new JSONScanner(response);
            parserState = 0;
            nestingLevel = 0;
            Distance = 0;
            Parsed = false;
            ConstructOriginalResponse = false;
            ID = -1; // default as not saved
        }

        /// <summary>
        /// Default constructor, based on a scanner already in progress. For example, it is used when parsing a Friends collection, which contains Person elements;
        /// or a user Wall, which contains Post elements
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        /// <param name="distance">indicates if the person is the user, a friend or other</param>
        public JSONParser(JSONScanner scanner)
        {
            OriginalResponse = "";
            ConstructOriginalResponse = true;
            temp = scanner;
            // { was already processed...
            parserState = 1;
            nestingLevel = 1;
            Distance = 0;
            Parsed = false;
        }

        public virtual void Parse()
        {
            // DEBUG
            //System.Windows.Forms.MessageBox.Show("Starting parse: " + this.GetType().ToString() );
            // parser states
            // 0 = Start, expecting {
            // 1 = expecting name
            // 2 = expecting :
            // 3 = expecting value or {, if value go to 4, if { go to 5
            // 4 = expecting , or }, if , go to 1, if } go to 0
            // 5 = call child parser - single element
            // 6 = call children parser - multiple element

            string name = "";
            string value = "";

            lastError = "";

            do
            {
                tokenId = temp.Scan(out token);
                if (ConstructOriginalResponse)
                {
                    OriginalResponse += token;
                }
                switch (parserState)
                {
                    case 0:
                        if ((tokenId != JSONScanner.JSONScannerTokens.PUNCTUATOR || token != "{") && tokenId != JSONScanner.JSONScannerTokens.EOF)
                        {
                            lastError += "Expected { at position " + temp.currentPosition + ", found " + tokenId + ":" + token + "\n";
                        }
                        else
                        {
                            parserState = 1;
                            nestingLevel++;
                        }
                        break;
                    case 1:
                        if (tokenId == JSONScanner.JSONScannerTokens.LITERAL)
                        {
                            parserState = 2;
                            name = token;
                        }
                        else
                        {
                            lastError += "Expected literal at position " + temp.currentPosition + ", found " + token + "\n";
                            // ignore for now
                            // TODO: more parsing
                        }
                        break;
                    case 2:
                        if ((tokenId != JSONScanner.JSONScannerTokens.PUNCTUATOR) || (token != ":"))
                        {
                            lastError += "Expected : at position " + temp.currentPosition + ", found " + token + "\n";
                            // TODO: return state?
                        }
                        else
                        {
                            parserState = 3;
                        }
                        break;
                    case 3:
                        switch (tokenId)
                        {
                            case JSONScanner.JSONScannerTokens.LITERAL:
                            case JSONScanner.JSONScannerTokens.IDENTIFIER:
                                parserState = 4;
                                value = token;
                                ProcessValue(name, value);
                                break;
                            case JSONScanner.JSONScannerTokens.NUMBER:
                                parserState = 4;
                                value = token;
                                ProcessNumericValue(name, value);
                                break;
                            case JSONScanner.JSONScannerTokens.PUNCTUATOR:
                                if (token == "{")
                                {
                                    parserState = 5; // getting a child object
                                    nestingLevel++;
                                }
                                else if (token == "[")
                                {
                                    parserState = 6; // getting an array of child objects
                                    nestingLevel++;
                                }
                                break;
                            default:
                                // ignore for now
                                // TODO: more parsing
                                break;
                        }
                        break;
                    case 4:
                        if ((tokenId != JSONScanner.JSONScannerTokens.PUNCTUATOR))
                        {
                            lastError += "Expected punctuator at position " + temp.currentPosition + ", found " + token + "\n";
                            // TODO: return state?
                        }
                        else if (token == "}")
                        {
                            parentName = "";
                            parserState = 0;
                            nestingLevel--;
                        }
                        else if (token == ",")
                        {
                            parserState = 1;
                        }
                        else
                        {
                            lastError += "Expected } or , at position " + temp.currentPosition + ", found " + token + "\n";
                            // TODO: return state?
                        }
                        break;
                    case 5:
                        parentName = name.Replace("\"", "");
                        string errorChild = ParseChild(token, temp, out tokenId, out token);
                        lastError += errorChild;

                        if ((tokenId != JSONScanner.JSONScannerTokens.PUNCTUATOR || token != "}") && tokenId != JSONScanner.JSONScannerTokens.EOF)
                            lastError += "Error processing child object: " + name;

                        parserState = 4;
                        break;
                    case 6:
                        int initialNesting = nestingLevel;

                        do
                        {
                            if (token == "{" || token == "[")
                            {
                                nestingLevel++;
                            }
                            if (token != "]") // empty children case, don't try to parse more
                            {
                                parentName = name.Replace("\"", "");
                                string errorChildren = ParseChildren(name, temp, out tokenId, out token);
                                lastError += errorChildren;
                            }
                            if (token == "}" || token == "]")
                            {
                                parentName = "";
                                parserState = 4; // wait another closure or a comma
                                nestingLevel--;
                            }
                            else if (token == ",")
                            {
                                parserState = 1;
                            }
                            else
                            {
                                lastError += "Expected } or , at position " + temp.currentPosition + ", found " + token + "\n";
                                // TODO: return state?
                            }
                        } while ( (nestingLevel != initialNesting - 1) && tokenId != JSONScanner.JSONScannerTokens.EOF );
                        break;
                }
            } while (tokenId != JSONScanner.JSONScannerTokens.EOF && nestingLevel != 0);
            // if there is more, scan for the next one
            if (tokenId == JSONScanner.JSONScannerTokens.PUNCTUATOR && (token == "}" || token == "]"))
            {
                tokenId = temp.Scan(out token);
                if (ConstructOriginalResponse)
                {
                    OriginalResponse += token;
                }
            }
            Parsed = true;
            // DEBUG
            //System.Windows.Forms.MessageBox.Show("End of parse: " + this.GetType().ToString() + "\n" + OriginalResponse);
            /*
            if ( this.GetType().ToString() == "FBCollection" )
            {
            System.Windows.Forms.MessageBox.Show("end of parsing: " + this.GetType().ToString() );
            FBCollection x = this as FBCollection;
            if ( x!= null )
            System.Windows.Forms.MessageBox.Show("of type " + x.itemType );

            }
            */

        }

        public virtual void Save(out string ErrorMessage)
        {
            ErrorMessage = "";
            ID = DBLayer.EntitySave(ID, Name, this.GetType().ToString(), true, Saved, out Saved, out ErrorMessage);
        }

        protected virtual string ParseChild(string name, JSONScanner temp, out JSONScanner.JSONScannerTokens tokenId, out string token)
        {
            string errors = "";
            int childParserState = 2;
            int initialNestingLevel = nestingLevel;
            string childName = "";
            string childValue = "";
            // parser states
            // 0 = Start, expecting { - NOT USED HERE
            // 1 = expecting name
            // 2 = expecting :

            childName = name; // default

            do
            {
                tokenId = temp.Scan(out token);
                if (ConstructOriginalResponse)
                {
                    OriginalResponse += token;
                }

                switch (childParserState)
                {
                    case 1:
                        if (tokenId == JSONScanner.JSONScannerTokens.LITERAL)
                        {
                            childParserState = 2;
                            childName = token;
                        }
                        else
                        {
                            lastError += "Expected LITERAL at position " + temp.currentPosition + ", found " + token + "\n";
                            // TODO: more parsing
                        }
                        break;
                    case 2:
                        if ((tokenId != JSONScanner.JSONScannerTokens.PUNCTUATOR) || (token != ":"))
                        {
                            lastError += "Expected : at position " + temp.currentPosition + ", found " + token + "\n";
                            // TODO: return state?
                        }
                        else
                        {
                            childParserState = 3;
                        }
                        break;
                    case 3:
                        switch (tokenId)
                        {
                            case JSONScanner.JSONScannerTokens.LITERAL:
                            case JSONScanner.JSONScannerTokens.IDENTIFIER:
                                childParserState = 4;
                                childValue = token;
                                ProcessValue(childName, childValue);
                                break;
                            case JSONScanner.JSONScannerTokens.NUMBER:
                                childParserState = 4;
                                childValue = token;
                                ProcessNumericValue(childName, childValue);
                                break;
                            case JSONScanner.JSONScannerTokens.PUNCTUATOR:
                                name = name.Replace("\"", "");
                                if (token == "[" && name == "data")
                                {
                                    nestingLevel++;
                                    string errorChildren = ParseChildren(parentName, temp, out tokenId, out token);
                                    childParserState = 4;
                                    lastError += errorChildren;
                                }
                                else
                                {
                                    lastError += "Expected value, not punctuator at position " + temp.currentPosition + ", found " + token + "\n";
                                    childParserState = 1;
                                }
                                break;
                            default:
                                lastError += "Expected LITERAL, IDENTIFIER, NUMBER OR PUNCTUATOR at position " + temp.currentPosition + ", found " + token + "\n";
                                // TODO: more parsing
                                break;
                        }
                        break;
                    case 4:
                        switch (tokenId)
                        {
                            case JSONScanner.JSONScannerTokens.PUNCTUATOR:
                                childParserState = 1;
                                break;
                            default:
                                lastError += "Expected punctuator at position " + temp.currentPosition + ", found " + token + "\n";
                                childParserState = 1;
                                break;
                        }
                        break;
                    default:
                        break;
                }
                if (token == "{")
                {
                    nestingLevel++;
                }
                else if (token == "}")
                {
                    nestingLevel--;
                    parentName = "";
                }
                else if (token == "]")
                {
                    nestingLevel--;
                }
            }
            while (tokenId != JSONScanner.JSONScannerTokens.EOF && nestingLevel != initialNestingLevel - 1);

            if (nestingLevel != initialNestingLevel - 1)
            {
                errors += "Unexpected end of file while parsing object " + name + "\n";
            }

            return errors;
        }

        protected string ParseChildren(string name, JSONScanner temp, out JSONScanner.JSONScannerTokens tokenId, out string token)
        {
            string errors = "";
            int initialNestingLevel = nestingLevel;
            // ASSUMPTION: fields should not have quotes, even in the middle
            name = name.Replace("\"", "");

            // Default, should never be required
            token = temp.LastToken;
            tokenId = JSONScanner.JSONScannerTokens.EOF;

            bool processed = false;
            if (ChildrenParsers != null)
            {

                // explore existing parsers
                // tODO: check foreach syntax
                foreach (ChildrenParse parser in ChildrenParsers)
                {
                    if (name == parser.Token)
                    {
                        string currentErrors = "";
                        GetChildrenByType(parser.ReferenceList, initialNestingLevel, parser.DataType, out tokenId, out token, out currentErrors);
                        errors += currentErrors;
                        processed = true;
                    }
                }
            }

            if (!processed)
            {
                // parsing a child object, recognize which one then 
                switch (name)
                {
                    case "to":
                        //System.Windows.Forms.MessageBox.Show("Processing to");
                        // Check: have to iterate until ] ?
                        do
                        {
                            tokenId = temp.Scan(out token);
                            if (ConstructOriginalResponse)
                            {
                                OriginalResponse += token;
                            }

                            if (tokenId == JSONScanner.JSONScannerTokens.PUNCTUATOR)
                            {
                                if (token == "[" || token == "{")
                                {
                                    nestingLevel++;
                                }
                                else if (token == "]" || token == "}")
                                {
                                    nestingLevel--;
                                }
                                else
                                {
                                    errors += "Unexpected punctuators found " + token + ", ignored\n";
                                }
                            }
                            else if (tokenId == JSONScanner.JSONScannerTokens.LITERAL)
                            {
                                // reset each time, since internally ParseChild removes it when going out of an element
                                parentName = name;
                                // Parse Child will reduce nesting level, since it iterates until }
                                ParseChild(token, temp, out tokenId, out token);
                            }
                            else
                            {
                                errors += "Unexpected error expecting punctuators or child name literal, found " + token + ", ignored\n";
                            }

                        }
                        // TODO: Double check where is initialNestingLevel and where -1, this may be the reason that cuts too many posts
                        while (tokenId != JSONScanner.JSONScannerTokens.EOF && nestingLevel != initialNestingLevel - 1);
                        // get the comma or { after...
                        if (tokenId != JSONScanner.JSONScannerTokens.EOF)
                        {
                            tokenId = temp.Scan(out token);
                        }
                        if (ConstructOriginalResponse)
                        {
                            OriginalResponse += token;
                        }

                        break;
                }
                errors += "Unknown object " + name + " ignored\n";
                IgnoreChildren(initialNestingLevel, out tokenId, out token);
            }

            // TODO : double check how it exits
            if (nestingLevel > initialNestingLevel)
            {
                errors += "Unexpected unbalance while parsing object " + name
            + ", level: " + nestingLevel
            + ", initial level: " + initialNestingLevel
            + "\n";
            }

            return errors;
        }

        protected void IgnoreChildren(int initialNestingLevel, out JSONScanner.JSONScannerTokens tokenId, out string token)
        {
            do
            {
                tokenId = temp.Scan(out token);
                if (tokenId == JSONScanner.JSONScannerTokens.PUNCTUATOR)
                {
                    if (token == "[" || token == "{")
                        nestingLevel++;
                    else if (token == "]" || token == "}")
                        nestingLevel--;
                }
            }
            while (tokenId != JSONScanner.JSONScannerTokens.EOF && nestingLevel != initialNestingLevel - 1);
            // get the comma or { after...
            if (tokenId != JSONScanner.JSONScannerTokens.EOF)
            {
                tokenId = temp.Scan(out token);
            }
        }

        private void ProcessValue(string name, string value)
        {
            // eliminate quotes
            // ASSUMPTION: fields should not have quotes, even in the middle
            name = name.Replace("\"", "");
            // ASSUMPTION: values only should eliminate initial and final quote
            if (value.Substring(0, 1) == "\"")
                value = value.Substring(1);
            if (value.Substring(value.Length - 1) == "\"")
                value = value.Substring(0, value.Length - 1);

            // TODO: Check all Unicode transformations - now in JSONScanner, test by removing this one
            // value = value.Replace("\\u0040", "@");

            AssignValue(name, value);
        }

        private void ProcessNumericValue(string name, string value)
        {
            // eliminate quotes
            // ASSUMPTION: fields should not have quotes, even in the middle
            name = name.Replace("\"", "");
            // ASSUMPTION: values are numberic and no quotes
            int intValue = 0;
            float floatValue = 0.0F;
            if (int.TryParse(value, out intValue))
            {
                AssignNumericValue(name, intValue);
            }
            else
                if (float.TryParse(value, out floatValue))
                {
                    AssignNumericValue(name, floatValue);
                }
        }

        protected virtual void AssignValue(string name, string value)
        {
            // Default: Nothing to do, only log the warning
            lastError += "Unknown name ignored: " + name + "\n";
        }

        protected virtual void AssignNumericValue(string name, int intValue)
        {
            // Default: Nothing to do, only log the warning
            lastError += "Unknown name ignored: " + name + "\n";
        }

        protected virtual void AssignNumericValue(string name, float floatValue)
        {
            // Default: Nothing to do, only log the warning
            lastError += "Unknown name ignored: " + name + "\n";
        }

        ArrayList GetChildrenByType(ArrayList list, int initialNestingLevel,
            string ChildrenType, out JSONScanner.JSONScannerTokens tokenId,
            out string token, out string errors)
        {
            int nChildren = 0;

            errors = "";
            token = "";
            tokenId = JSONScanner.JSONScannerTokens.ERROR;

            // at this point, it should have got to the <name>{data:[
            if (temp.LastToken != "{")
            {
                // move to { to get to the first comment
                tokenId = temp.Scan(out token);
                if (ConstructOriginalResponse)
                {
                    OriginalResponse += token;
                }
            }
            if (temp.LastToken == "{")
            {
                nestingLevel++;
                do
                {
                    JSONParser item;
                    switch (ChildrenType)
                    {
                        case "FBMessage":
                            //System.Windows.Forms.MessageBox.Show("message: " + nChildren + " at position " + temp.currentPosition );
                            item = new FBMessage(temp, this);
                            break;
                        case "FBWork":
                            item = new FBWork(temp, this);
                            break;
                        case "FBEducation":
                            item = new FBEducation(temp, this);
                            break;
                        case "FBPerson":
                            item = new FBPerson(temp, Distance, this);
                            break;
                        case "FBPost":
                            item = new FBPost(temp, this);
                            break;
                        case "FBAlbum":
                            item = new FBAlbum(temp);
                            break;
                        case "FBPhoto":
                            item = new FBPhoto(temp, parentID, parentSNID);
                            break;
                        case "FBTag":
                            item = new FBTag(temp, this);
                            break;
                        case "FBEvent":
                            item = new FBEvent(temp, this);
                            break;
                        case "FBCollection":
                            item = new FBCollection(temp, "FBPerson", parentID, parentSNID);
                            break;
                        case "FBFriendList":
                            // TODO: Add parent, parentSNID
                            item = new FBFriendList(temp);
                            break;
                        default:
                            errors += "don't know how to process " + ChildrenType + "\n";
                            item = null;
                            break;
                    }
                    if (item != null)
                    {
                        item.Parse();
                        list.Add(item);
                        nChildren++;
                    }

                    // TODO: triple check validity of list sequence
                    tokenId = JSONScanner.JSONScannerTokens.PUNCTUATOR;
                    token = temp.LastToken;
                    if (token == ",")
                    {
                        tokenId = temp.Scan(out token);
                        if (ConstructOriginalResponse)
                        {
                            OriginalResponse += token;
                        }
                    }
                    if (token == "]" || token == "}")
                    {
                        nestingLevel--;
                    }
                } while (tokenId != JSONScanner.JSONScannerTokens.EOF && token == "{");
            }
            else
            {
                // default process
                errors += "Unexpected error expecting {, found " + temp.LastToken + ", ignored\n";
                if ( temp.LastTokenId != JSONScanner.JSONScannerTokens.EOF )
                {
                    IgnoreChildren(initialNestingLevel, out tokenId, out token);
                }
            }
            return list;
        }

        protected void AddParser(string token, string datatype, ref ArrayList reference)
        {
            if (ChildrenParsers == null)
            {
                ChildrenParsers = new ArrayList();
            }
            ChildrenParse temp = new ChildrenParse(token, datatype, ref reference);
            ChildrenParsers.Add(temp);
        }

    }

    /// <summary>
    /// Simple class that allows to list the tokens associated to children, the ArrayList that is assigned to them, 
    /// and the data type used to parse it
    /// </summary>
    public class ChildrenParse
    {
        public string Token { get; set; }
        public string DataType { get; set; }
        public ArrayList ReferenceList { get; set; }

        public ChildrenParse(string token, string datatype, ref ArrayList reference)
        {
            Token = token;
            DataType = datatype;
            reference = new ArrayList();
            ReferenceList = reference;
        }
    }

}
