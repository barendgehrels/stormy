License: The MIT License (MIT) Copyright (c) 2010..2012 Barend Gehrels

Permission is hereby granted, free of charge, to any person obtaining a
copy of this software and associated documentation files (the "Software"),
to deal in the Software without restriction, including without limitation
the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
DEALINGS IN THE SOFTWARE.

Several methods to do one-to-many or many-to-many relationships:

Many-to-many is explained below and in the samples.
1: read both lists, read link table and link them. Good for many-to-many. 3 queries in total
2: read list while also reading subrecords. Good for one-to-many, will create copies. 2 queries in total
3: read list and after that read subrecords with a separate query. 3 queries in total
4: do all in once, read full outer joined table. Good for many-to-many where things are linked to each other. 1 query in total.
5: do a sub-query for each record. Bad for performance, will create deep copies for each mouse. 2 + N queries. 
