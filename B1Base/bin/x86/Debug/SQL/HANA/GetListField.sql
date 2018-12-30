select 'U_' + "AliasID" + ' as ' + "AliasID"
from CUFD
where (upper("TableID") = upper('@{0}'))