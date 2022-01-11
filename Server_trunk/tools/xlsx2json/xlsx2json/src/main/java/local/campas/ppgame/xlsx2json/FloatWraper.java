package local.campas.ppgame.xlsx2json;

import org.json.JSONString;

public class FloatWraper implements JSONString{
	private Number number;
	public FloatWraper(Number number){
		this.number = number;
	}
	
	@Override
	public String toJSONString(){
		String str = number.toString();
		if(-1 == str.indexOf('.')){
			str += ".0";
		}
		return str;
	}	
	
	public static boolean isNum(String str){
		return str.matches("^[-+]?(([0-9]+)([.]([0-9]+))?|([.]([0-9]+))?)$");
	}
}
