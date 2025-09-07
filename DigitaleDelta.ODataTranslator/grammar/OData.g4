grammar OData;

query
    : queryOption (AMPERSAND queryOption)* EOF
    ;

queryOption
    : filterOption
    | selectOption
    | topOption
    | countOption
    | skipTokenOption
    ;

filterOption
    : FILTER EQ filterExpr
    ;

primary
    : IDENTIFIER
    | STRING
    | NUMBER
    ;

valueList
    : primary (COMMA primary)*
    ;

/* 
 * Note about IN operator:
 * The IN operator is case-insensitive ([iI][nN]) and has precedence issues when parsing expressions.
 * It needs to be carefully positioned in the filterExpr rule to ensure proper evaluation order.
 * Be aware that "in" could be part of an identifier if we weren't handling it as a keyword.
 */
filterExpr
    : primary
    | LPAREN filterExpr RPAREN
    | function
    | filterExpr IN LPAREN valueList RPAREN
    | filterExpr comparison filterExpr
    | NOT filterExpr
    | filterExpr AND filterExpr
    | filterExpr OR filterExpr
    ;

function
    : STARTSWITH LPAREN filterExpr COMMA filterExpr RPAREN
    | ENDSWITH LPAREN filterExpr COMMA filterExpr RPAREN
    | CONTAINS LPAREN filterExpr COMMA filterExpr RPAREN
    | TOLOWER LPAREN filterExpr RPAREN
    | TOUPPER LPAREN filterExpr RPAREN
    | LENGTH LPAREN filterExpr RPAREN
    | INDEXOF LPAREN filterExpr COMMA filterExpr RPAREN
    | SUBSTRING LPAREN filterExpr COMMA filterExpr (COMMA filterExpr)? RPAREN
    | TRIM LPAREN filterExpr RPAREN
    | YEAR LPAREN filterExpr RPAREN
    | MONTH LPAREN filterExpr RPAREN
    | DAY LPAREN filterExpr RPAREN
    | HOUR LPAREN filterExpr RPAREN
    | MINUTE LPAREN filterExpr RPAREN
    | SECOND LPAREN filterExpr RPAREN
    | NOW LPAREN RPAREN
    | DATE LPAREN filterExpr RPAREN
    | TIME LPAREN filterExpr RPAREN
    | DISTANCE LPAREN filterExpr RPAREN
    | INTERSECTS LPAREN STRING RPAREN
    | FLOOR LPAREN filterExpr COMMA filterExpr RPAREN
    | CEIL LPAREN filterExpr COMMA filterExpr RPAREN
    | ROUND LPAREN filterExpr COMMA filterExpr RPAREN
    | ABS LPAREN filterExpr COMMA filterExpr RPAREN
    ;

comparison
    : GT
    | LT
    | GE
    | LE
    | EQ_OP
    | NE
    ;

selectOption
    : SELECT EQ selectItem (COMMA selectItem)*
    ;

selectItem
    : IDENTIFIER
    ;

topOption
    : TOP EQ NUMBER
    ;

countOption
    : COUNT EQ BOOLEAN
    ;

skipTokenOption
    : SKIPTOKEN EQ STRING
    ;

FILTER    : '$filter' ;
SELECT    : '$select' ;
TOP       : '$top' ;
COUNT     : '$count' ;
SKIPTOKEN : '$skiptoken' ;

STARTSWITH : 'startswith' ;
ENDSWITH   : 'endswith' ;
CONTAINS   : 'contains' ;
TOLOWER    : 'tolower' ;
TOUPPER    : 'toupper' ;
LENGTH     : 'length' ;
INDEXOF    : 'indexof' ;
SUBSTRING  : 'substring' ;
TRIM       : 'trim' ;
TIME       : 'time' ;
FLOOR      : 'floor' ;
CEIL       : 'ceil' ;
ROUND      : 'round' ;
ABS        : 'abs' ;
YEAR       : 'year' ;
MONTH      : 'month' ;
DAY        : 'day' ;
HOUR       : 'hour' ;
MINUTE     : 'minute' ;
SECOND     : 'second' ;
NOW        : 'now' ;
DATE       : 'date' ;
DISTANCE   : 'distance' ;
INTERSECTS : 'intersects' ;

// Case-insensitive keywords to match OData specification
EQ        : '=' ;
GT        : [gG][tT] ;
LT        : [lL][tT] ;
GE        : [gG][eE] ;
LE        : [lL][eE] ;
EQ_OP     : [eE][qQ] ;
NE        : [nN][eE] ;
AND       : [aA][nN][dD] ;
OR        : [oO][rR] ;
NOT       : [nN][oO][tT] ;
IN        : [iI][nN] ;

LPAREN    : '(' ;
RPAREN    : ')' ;
COMMA     : ',' ;
AMPERSAND : '&' ;

BOOLEAN   : 'true' | 'false' ;
NUMBER    : '-'? DIGIT+ ('.' DIGIT+)? ;
STRING    : QUOTE .*? QUOTE ;
IDENTIFIER: ALPHA (ALPHA | DIGIT | '_' | '/')* ;


fragment ALPHA : [a-zA-Z] ;
fragment DIGIT : [0-9] ;
fragment QUOTE : '\'' ;

WS : [ \t\r\n]+ -> skip ;
