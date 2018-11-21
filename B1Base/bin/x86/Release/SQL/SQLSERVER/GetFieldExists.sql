select count(*) 
from CUFD
where (TableId = '@{0}' or AliasID = '{0}')
and AliasId = '{1}'