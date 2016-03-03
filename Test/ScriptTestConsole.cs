using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class ScriptTestConsole
    {
        StringBuilder fStringBuffer = new StringBuilder();
        DateTime _dateTime;
        //var     bag: Dictionary<System.Object, System.Object> := new Dictionary<System.Object, System.Object>();
        //
        //public
        //constructor ;
        //
        //property propDouble: System.Double read 1.2 write set_propDouble;
        //method set_propDouble(value: System.Double);
        //method getDouble: System.Double;
        //method writeDouble(value: System.Double);
        //method getDateAsDouble: System.Double;
        //method getDateAsInt: System.Int32;
        //
        //property propInt: System.Int32 read 150 write set_propInt;
        //method set_propInt(value: System.Int32);
        //method getInt: System.Int32;
        //method writeInt(value: System.Int32);
        //
        //property propString: System.String read 'string.value' write set_propString;
        //method set_propString(value: System.String);
        //method writeString(value: System.String);
        //method getString: System.String;
        //
        //property propBoolean: System.Boolean read true write set_propBoolean;
        //method set_propBoolean(value: System.Boolean);
        //method writeBoolean(value: System.Boolean);
        //method getBoolean: System.Boolean;
        //
        //property propDate: DateTime read new DateTime(1974, 1, 1) write set_propDate;
        //method set_propDate(value: DateTime);
        //method writeDate(value: DateTime);
        //method getDate: DateTime;
        //
        //property propObject: System.Object read self write set_propObject;
        //method set_propObject(value: System.Object);
        //
        //method writeObject(value: System.Object);
        //
        //method ToString: System.String; override;
        //
        //method load(commandText: System.String; params parameters: array of System.Object);
        //
        //method load2(commandText: System.String; parameters: array of System.Object);
        //
        //method loadStrings(params parameters: array of System.String);
        //method loadDoubles(params parameters: array of System.Double);
        //method loadStrings2(parameters: array of System.String);
        //
        //property Item[name: System.String]: System.Object read get_Item write bag[name]; default;
        //method get_Item(name: System.String): System.Object;
        //
        //property Item[&index: System.Int32]: System.Object read get_Item write bag[&index.ToString()]; default;
        //method get_Item(&index: System.Int32): System.Object;
        //
        //method ThrowException;
        public ScriptTestConsole()
        { }


        public object propObject { get; set; }
        public bool propBoolean { get; set; }
        public DateTime propDate
        {
            get
            {
                return _dateTime;
            }
            set
            {
                _dateTime = value;
            }
        }
        public Double propDouble { get; set; }
        public string propString { get; set; }

        public void writeString(string s)
        {
            fStringBuffer.Append(s);
            fStringBuffer.Append("||");
        }
        public void writeObject(object o)
        {
            propObject = o;
        }

        public void throwException()
        {
            throw new Exception("Test Message");
        }

        public override string ToString()
        {
            return "Custom .ToString call result";
        }

        public string GetStringBuffer()
        {
            return fStringBuffer.ToString();
        }
    }
}
