select coalesce(
(select  case when upper(table_name) = upper('@{0}') then 1 when upper(table_name) = upper('{0}') then 2 end
from tables  
where (upper(table_name) = upper('@{0}') or upper(table_name) = upper('{0}'))
and schema_name = current_schema), 0) from dummy