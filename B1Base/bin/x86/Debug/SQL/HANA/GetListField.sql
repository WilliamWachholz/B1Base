select count(*)
from "CUFD"
where (upper("TableID") = upper('@{0}') or upper("TableId") = upper('{0}'))
and upper("AliasID") = upper('{1}')