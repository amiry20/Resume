using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Application.Utilities
{
    public   class CommandHelper
    {
        public static string GetCanDeleteCommand(string tableName, long id)
        { 
            return @$"  
 Declare @Query varchar(max) 
 Set  @Query = ''  
 Select   
   @Query = case @Query When '' then '' else @Query + ' Union All ' end + 'Select ''' + totable.name +''' As [Title] , Count(*) As [Count] From [' + totable.name +'] Where [' + syscolumn.name +'] = ' + cast( {id} as varchar)  
 FROM sys.foreign_key_columns AS sysfkey 
 Left join sys.tables fromtable  on fromtable.object_id = sysfkey.referenced_object_id
 Left join sys.tables totable on  totable.object_id = sysfkey.parent_object_id
 Left join sys.columns syscolumn on syscolumn.column_id = sysfkey.parent_column_id And syscolumn.object_id = sysfkey.parent_object_id 
 where fromtable.name = '{tableName}'  
 EXEC( @Query ) 
";

        }
    }
}
