select 'U_' + AliasID + ' as ' + AliasID
from CUFD
where (TableId = '@{0}')