<#using# SharpTemplate.Test.Resources#>
<#using#SharpTemplate.Test#>
<#model# ParseCompileModel#>
using System;
using System.Collections.Generic;

namespace TestNamespace
{
	public class <# Write(model.ClassName); #>{

		public  <# Write(model.ClassName); #>()
		{
		
		}

		public List<string> <# Write(model.MethodName); #>()
		{
			var result = new List<string>();
			for(int i = 0;i<<# Write(model.UpTo.ToString()); #>;i++){
				result.Add(i.ToString());
			}
			return result;
		}
	}
}