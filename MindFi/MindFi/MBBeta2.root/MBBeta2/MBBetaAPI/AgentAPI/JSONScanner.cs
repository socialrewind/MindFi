using System;
using System.Text;
using System.IO;
using System.Globalization;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Generic class to do lexical analysis for JSON strings as returned by Facebook among other web services
    /// </summary>
    /// 
    public class JSONScanner
    {

        #region "ConstInitializers"

        // char families
        const int ALPHA=0;
        const int DIGIT = 1;
        const int OPER = 2;
        const int PUNC = 3;
        const int SPACE = 4;
        const int OTHER = 5;
        const int QUOTE = 6;
        const int NEWLINE = 7;
        const int BACKSLASH = 8;
        const int EOF = 15;

        private string alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_@?$%";
        private string digit = "01234567890";
        private string punc = "{}[].,:+-";
        private string quote = "\"\'";
        private string space = "\t ";
        private string newline = "\n\r";
        private char backslash = '\\';
        private char eof = (char)26;

        // results
        public enum JSONScannerTokens 
            {
                ERROR=0, 
                IDENTIFIER=1,
                NUMBER=2,
                OPERATOR=3,
                PUNCTUATOR=4,
                LITERAL=5,
                ENDOFLINE=6,
                NEWLINE=7,
                BACKSLASH=8,
                EOF=9
            };

        const int BUFFERSIZE = 1024;
        const int NUMBUFFERS = 2;
	const int ASCIISIZE = 256;

        #endregion

        #region "Internal variables"

        private int[] m_table; // Note: this is extended ASCII, not Unicode

        // Double Buffer management
        private char[][] buffer;
        private string originalInput;
        private Stream originalStream;
        private int currentChar = 0;
        private int currentCharInInput = 0;
        private int[] bufferLimit;
        private bool endOfFile = false;
        private int currentBuffer = 0;
        private int lastBuffer = 0;
        private int m_nextBuffer = 0;

        private string m_lastToken;
        private JSONScannerTokens m_lastTokenId;

        private int nextBuffer
        {
            get
            {
                return m_nextBuffer;
            }

            set
            {
                m_nextBuffer = value + 1;
                if (m_nextBuffer >= NUMBUFFERS)
                    m_nextBuffer = 0;
            }
        }

	private int table(char current)
	{
	    // TODO: Unicode case is alpha
	    if ( (int)current < ASCIISIZE )
		return m_table[(int)current];
	    else
		return ALPHA;
	}

        /// <summary>
        /// Returns last (previously) scanned token
        /// </summary>
        public string LastToken { get { return m_lastToken; } }
        /// <summary>
        /// Returns last (previously) scanned token ID
        /// </summary>
        public JSONScannerTokens LastTokenId { get { return m_lastTokenId; } }
        #endregion

        #region "Constructors"

        public JSONScanner()
        {
            Init();
        }

        public JSONScanner(string input)
        {
            Init();
            originalInput = input;
            originalStream = null;
            ReadBuffers(true);
        }

        public JSONScanner(Stream input)
        {
            Init();
            originalStream = input;
            originalInput = null;
            ReadBuffers(true);
        }

        #endregion

        #region "Internal Methods"

        private void Init()
        {
            InitializeBuffer();
            FillTable();
        }

        #region "Buffer management"
        private void InitializeBuffer()
        {
            buffer = new char[NUMBUFFERS][];
            for (int i = 0; i < NUMBUFFERS; i++)
                buffer[i] = new char[BUFFERSIZE];
            bufferLimit = new int[NUMBUFFERS];
        }

        private void ReadBuffers(bool touchCurrentBuffer)
        {
            nextBuffer = currentBuffer;
            if (touchCurrentBuffer)
                ReadNextBlockToBuffer(currentBuffer);
            int temp = nextBuffer;
            while (temp != currentBuffer)
            {
                ReadNextBlockToBuffer(temp);
                // TODO: improve and make readable
                // TRICKY, nextBuffer is a property...
                nextBuffer = temp;
                temp = nextBuffer;
            }
        }

        private void ReadNextBlockToBuffer(int bufferToRead)
        {
            if (endOfFile)
                return;

            int howMuchToRead;
            if (originalStream != null)
            {
                // read from stream
                
                byte[] ByteBuffer = new byte[BUFFERSIZE];                
                howMuchToRead = originalStream.Read(ByteBuffer, 0, BUFFERSIZE);
                buffer[bufferToRead] = Encoding.ASCII.GetChars(ByteBuffer);
                if (howMuchToRead > buffer[bufferToRead].Length)
                    throw new Exception("size for buffer read differs and is not the end");
                bufferLimit[bufferToRead] = howMuchToRead;
                currentCharInInput += howMuchToRead;
                lastBuffer = bufferToRead;

                if (currentCharInInput >= originalStream.Length)
                    endOfFile = true;
            }
            else
            {
                // read from string
                howMuchToRead = originalInput.Length - currentCharInInput;
                if (howMuchToRead > BUFFERSIZE)
                    howMuchToRead = BUFFERSIZE;

                buffer[bufferToRead] = originalInput.Substring(currentCharInInput, howMuchToRead).ToCharArray();
                bufferLimit[bufferToRead] = howMuchToRead;
                currentCharInInput += howMuchToRead;
                lastBuffer = bufferToRead;

                if (currentCharInInput >= originalInput.Length)
                    endOfFile = true;
            }
        }

        #endregion

        private void FillTable()
        {
            // Initialize table
            m_table = new int[ASCIISIZE];
            for (int i = 0; i < m_table.Length; i++) m_table[i] = OTHER;

            FillTable(alpha, ALPHA);
            FillTable(digit, DIGIT);
            FillTable(punc, PUNC);
            FillTable(quote, QUOTE);
            FillTable(space, SPACE);
            FillTable(newline, NEWLINE);
            m_table[0] = NEWLINE;
            m_table[(int)backslash] = BACKSLASH;
            m_table[eof] = EOF;
        }

        private void FillTable(string chars, int value)
        {
            char[] temp = chars.ToCharArray();

            for (int j = 0; j < temp.Length; j++)
            {
                char current = temp[j];

                m_table[(int)current] = value;
            }

        }
        char CurrentInput()
        {
            if (currentChar < bufferLimit[currentBuffer])
            {
                return buffer[currentBuffer][currentChar];
            }

            //if (endOfFile)
            return eof;
        }

        char NextInput()
        {
            if (++currentChar < bufferLimit[currentBuffer])
                return buffer[currentBuffer][currentChar];

            if (endOfFile && currentBuffer == lastBuffer )
                return eof;
            // 
            currentChar = 0;
            // TRICKY, nextBuffer is a property...
            int previousBuffer = currentBuffer;
            nextBuffer = currentBuffer;
            currentBuffer = nextBuffer;
            ReadNextBlockToBuffer(previousBuffer);
            return buffer[currentBuffer][currentChar];
        }

	char ProcessUnicode(out StringBuilder tempToken)
	{
	    char current;

	    tempToken = new StringBuilder();
	    tempToken.Append( CurrentInput() );
	    current = NextInput();
	    // Process Unicode case
	    switch ( current )
	    {
		case 'u':
		    tempToken.Append(current);
		    StringBuilder hexToken = new StringBuilder();
		    // parse exactly 4 hex chars, err if less
		    for ( int HexChar=0; HexChar<4; HexChar++)
		    {
		    current = NextInput();
		    tempToken.Append(current);
		    if ( current<'0' || ( current>'9' && current <'A') || ( current>'Z' && current <'a')|| current>'z' )
		    {
			return backslash;
		    }
		    hexToken.Append(current);
		    }
		    current = (char) int.Parse(hexToken.ToString(), NumberStyles.HexNumber);
		    //System.Windows.Forms.MessageBox.Show("Ready to return " + current + " converted from " + tempToken);
		    // convert all unicode into a single char to add to the token
		    return current;
		case 'n':
		    return '\n';
		case 'r':
		    return '\r';
        case 't':
            return '\t';
        case '\\':
		    return '/'; // TEST
		case '"':
		case '/':
		    return current;
	    }
//System.Windows.Forms.MessageBox.Show("after backslash : " + current);
// ignore backslash, convert to the character after w/o backslash
	    return current;
	}

        #endregion

        #region "Public Members"

        public int currentPosition { get { return currentChar; } }

        public JSONScannerTokens Scan(out string scannerOutput)
        {
            StringBuilder token = new StringBuilder();
	    StringBuilder tempToken;

            m_lastTokenId = JSONScannerTokens.ERROR;

Top:
            char current = CurrentInput();

            switch (table(current))
            {
                case ALPHA:
                    token.Append(current);
                    do
                    {
                        current = NextInput();
                        if (table(current) <= DIGIT)
			{
                            token.Append(current);
			}
/*
			else
			{
                        if (current == backslash)
			{
//System.Windows.Forms.MessageBox.Show("Entering Alpha case");
                            current = ProcessUnicode(out tempToken);
			    if ( current == backslash )
			    { 
				scannerOutput = tempToken.ToString();
                    		m_lastToken = scannerOutput;
                    		m_lastTokenId = JSONScannerTokens.ERROR;
            			return m_lastTokenId;
			    }
                            token.Append(current);
			    // TODO: Make sure this case doesn't end the loop
			}
			}
*/
		    } while (table(current) <= DIGIT);
                    scannerOutput = token.ToString();
                    m_lastToken = scannerOutput;
                    m_lastTokenId = JSONScannerTokens.IDENTIFIER;
                    break;
                case DIGIT:
                    token.Append(current);
                    do
                    {
                        current = NextInput();
                        if (table(current) == DIGIT)
                            token.Append(current);
                    } while (table(current) == DIGIT);
                    if (current == '.')
                    {
                        bool moreDigits = false;
                        token.Append(current);
                        do
                        {
                            current = NextInput();
                            if (table(current) == DIGIT)
                            {
                                token.Append(current);
                                moreDigits = true;
                            }
                        } while (table(current) == DIGIT);
                        if (!moreDigits)
                        {
                            scannerOutput = token.ToString();
                            m_lastToken = scannerOutput;
                            m_lastTokenId = JSONScannerTokens.ERROR;
                            return m_lastTokenId;
                        }
                    }
                    scannerOutput = token.ToString();
                    m_lastToken = scannerOutput;
                    m_lastTokenId = JSONScannerTokens.NUMBER;
                    break;
                case SPACE:
                case NEWLINE:
                    while (table(current) == SPACE || table(current) == NEWLINE)
                    {
                        current = NextInput();
                    }
                    goto Top;
                    //break;
                case PUNC:
                    bool signFlag = false;
                    token.Append(current);
                    if (current == '-' || current == '+')
                    {
                        signFlag = true;
                    }
                    current = NextInput();
                    if (signFlag && table(current)==DIGIT)
                        goto Top;
                    scannerOutput = token.ToString();
                    m_lastToken = scannerOutput;
                    m_lastTokenId = JSONScannerTokens.PUNCTUATOR;
                    break;
                case QUOTE:
                    char startLiteral = current;
                    char previous = eof;

                    token.Append(current);
                    do
                    {
                        previous = current;
                        current = NextInput();
                        if (current == backslash)
			{
			    //System.Windows.Forms.MessageBox.Show("Entering Quote case");
			    previous = current; // keep same exit condition
                            current = ProcessUnicode(out tempToken);
			    if ( current == backslash )
			    { 
				scannerOutput = tempToken.ToString();
                    		m_lastToken = scannerOutput;
                    		m_lastTokenId = JSONScannerTokens.ERROR;
            			return m_lastTokenId;
			    }
                            token.Append(current);
			    //System.Windows.Forms.MessageBox.Show("Token on quote case:" + token);
			} else 
			{
			    token.Append(current);
			}
                    } while (
                        ( current != startLiteral || ( previous == backslash && current == startLiteral ) )
                        && table(current) != EOF );
                    if (table(current) == EOF)
                    {
                        scannerOutput = token.ToString();
                        m_lastToken = scannerOutput;
                        m_lastTokenId = JSONScannerTokens.ERROR;
                    } else
                    {
                        scannerOutput = token.ToString();
                        // advance after quote
                        current = NextInput();
                        m_lastToken = scannerOutput;
                        m_lastTokenId = JSONScannerTokens.LITERAL;
                    }
                    break;
                case EOF:
                    current = NextInput();
                    scannerOutput = eof.ToString();
                    m_lastToken = scannerOutput;
                    m_lastTokenId = JSONScannerTokens.EOF;
                    break;
/*
                case BACKSLASH:
//System.Windows.Forms.MessageBox.Show("Entering Backslash case");
                    current = ProcessUnicode(out tempToken);
                    if ( current == '\\' )
                    {
			scannerOutput = tempToken.ToString();
                    	m_lastToken = scannerOutput;
                    	m_lastTokenId = JSONScannerTokens.ERROR;
            		return m_lastTokenId;
		    }
		    token.Append(current);
		    goto Top;
*/
                default:
                    current = NextInput();
                    scannerOutput = token.ToString();
                    m_lastToken = scannerOutput;
                    m_lastTokenId = JSONScannerTokens.ERROR;
                    break;
            }

            return m_lastTokenId;
        }

        #endregion
    }
}
