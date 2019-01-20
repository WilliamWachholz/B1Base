select count(*) 
from CUFD
where (TableId = '@{0}' or TableId = '{0}')
and AliasId = '{1}'