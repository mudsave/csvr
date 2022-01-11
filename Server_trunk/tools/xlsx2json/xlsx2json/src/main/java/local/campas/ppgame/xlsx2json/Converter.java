package local.campas.ppgame.xlsx2json;


import java.math.BigDecimal;

import org.json.JSONArray;
import org.json.JSONObject;

import com.google.common.base.Strings;

public enum Converter {
    funcInt {
        @Override
        public Object convert(final String valueString) {
            return new BigDecimal(valueString).intValueExact();
        }

        @Override
        public Object defaultValue() {
            return 0;
        }
    },
    funcInt64 {
        @Override
        public Object convert(final String valueString) {
            return new BigDecimal(valueString).longValue();
        }

        @Override
        public Object defaultValue() {
            return 0;
        }
    },    
    funcStr {
        @Override
        public Object convert(final String valueString) {
            return valueString;
        }

        @Override
        public Object defaultValue() {
            return "";
        }
    },
    funcIntMapInt {
        @Override
        public Object convert(final String valueString) {
            JSONObject jsonObject = new JSONObject();
            if (!Strings.isNullOrEmpty(valueString)) {
                for (String kv : valueString.split(",")) {
                    String[] split = kv.split("=");
                    jsonObject.put(split[0], funcInt.convert(split[1]));
                }
            }
            return jsonObject;
        }

        @Override
        public Object defaultValue() {
            return new JSONObject();
        }
    },
    funcFloat {
        @Override
        public Object convert(final String valueString) {
            return new FloatWraper(new BigDecimal(valueString).doubleValue());
        }

        @Override
        public Object defaultValue() {
            return new FloatWraper(0.0);
        }
    },
    funcTupleInt {
        @Override
        public Object convert(final String valueString) {
            JSONArray jsonArray = new JSONArray();
            if (!Strings.isNullOrEmpty(valueString)) {
                for (String value : valueString.split(",")) {
                    jsonArray.put(funcInt.convert(value));
                }
            }
            return jsonArray;
        }

        @Override
        public Object defaultValue() {
            return new JSONArray();
        }
    },
    funcBool {
        @Override
        public Object convert(final String valueString) {
            return "1".equals(valueString);
        }

        @Override
        public Object defaultValue() {
            return false;
        }
    },
    funcMapFloat {
        @Override
        public Object convert(final String valueString) {
            JSONObject jsonObject = new JSONObject();
            if (!Strings.isNullOrEmpty(valueString)) {
                for (String kv : valueString.split(",")) {
                    String[] split = kv.split("=");
                    jsonObject.put(split[0], funcFloat.convert(split[1]));
                }
            }
            return jsonObject;
        }

        @Override
        public Object defaultValue() {
            return new JSONObject();
        }
    },
    funcMapJson {
        @Override
        public Object convert(final String valueString) {
 
            String value = String.format("{%s}", valueString);
            
            return new JSONObject(value);

        }

        @Override
        public Object defaultValue() {
            return new JSONObject();
        }
    },
    funcListJson {
        @Override
        public Object convert(final String valueString) {
        	
        	String value = String.format("[%s]", valueString);

            return new JSONArray(value);
        }

        @Override
        public Object defaultValue() {
            return new JSONArray();
        }
    },
    funcMapData {
        @Override
        public Object convert(final String valueString) {
        	
            String value = valueString.replace(';', ',');
            return funcMapJson.convert(value);
        }

        @Override
        public Object defaultValue() {
            return new JSONObject();
        }
    },     
    funcListData {
        @Override
        public Object convert(final String valueString) {        	
        	
        	String value = "";
        	
        	if (!Strings.isNullOrEmpty(valueString)) {
        		String [] splitStr = valueString.split(";");
        		for(int i=0; i<splitStr.length; i++){
        			String valuestr = splitStr[i].replace(':', ',');
        			if (i == splitStr.length-1)
        				value = value + "[" + valuestr + "]";
        			else 
        				value = value + "[" + valuestr + "],";
        		}
        	}
        	
        	 return funcListJson.convert(value);
        }

        @Override
        public Object defaultValue() {
            return new JSONArray();
        }
    },     
    funcTupleFloat {
        @Override
        public Object convert(final String valueString) {
            JSONArray jsonArray = new JSONArray();
            if (!Strings.isNullOrEmpty(valueString)) {
                for (String value : valueString.split(",")) {
                    jsonArray.put(funcFloat.convert(value));
                }
            }
            return jsonArray;
        }

        @Override
        public Object defaultValue() {
            return new JSONArray();
        }
    },
    funcZipFloat{
        @Override
        public Object convert(final String valueString) {
            return (new BigDecimal(valueString).doubleValue())*10000;
        }

        @Override
        public Object defaultValue() {
            return 0;
        }   	
    };

    public abstract Object convert(String valueString);

    public abstract Object defaultValue();
    
}


