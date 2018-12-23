select count(*) 
from tables  
where upper(table_name) = upper('@{0}')
and schema_name = current_schema