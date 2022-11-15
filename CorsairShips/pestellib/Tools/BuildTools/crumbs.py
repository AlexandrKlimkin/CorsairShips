import argparse
import os
import re

parser = argparse.ArgumentParser(description='Generates cross-links in footer for all html pages in specific dir')
parser.add_argument('--dir', type=str, required=True, help='target directory')
parser.add_argument('--exts', type=str, default='aspx', help='html files extensions (eg. "aspx;html")')

def generateLinks(exclude, pages):
    html = ''
    for p in pages:
        if p['name'] in exclude:
            html += p['name']
        else:
            html += '<a href="%s">%s</a>' % (p['path'], p['name'])
        if len(html) > 0:
            html += ' :: '
    return '<div id="generated-footer"> :: %s</div>' % html

args = parser.parse_args()
exts = args.exts.split(';')
foundPages = []
for entry in os.listdir(args.dir):
    for e in exts:
        l = len(e)
        if entry.lower()[-l:] == e.lower():
            foundPages.append(
                {'path': entry,
                 'name': entry[:-(l+1)],
                 'fullPath': os.path.join(args.dir, entry)
                 }
            )
            print 'Adding menu to ' + entry

autoGenBegin = '\n<!-- START OF AUTO GENERATED CONTENT (see crumps.py) -->\n'
autoGenEnd = '\n<!-- END OF AUTO GENERATED CONTENT (see crumps.py) -->\n'
rgxOldFooter = re.compile('(.*?<body>)(.*?<div id="generated-footer">.*?</div>.*?)(<[\\w%].*)', re.IGNORECASE | re.MULTILINE | re.DOTALL)
rgxNewFooter = re.compile('(.*?<body>)(.*?)(<.*)', re.IGNORECASE | re.MULTILINE | re.DOTALL)

for p in foundPages:
    with open(p['fullPath'], 'r') as f:
        data = f.read()

    m = None
    mOld = rgxOldFooter.match(data)
    mNew = rgxNewFooter.match(data)
    if mOld is not None:
        m = mOld
    elif mNew is not None:
        m = mNew

    if m is None:
        print "Can't find entry point for footer in file '%s'" % p['path']
        break

    up = m.group(1)
    links = generateLinks([p['name']], foundPages)
    bottom = m.group(3)

    with open(p['fullPath'], 'w') as f:
        pass
        f.writelines(up + autoGenBegin + links + autoGenEnd + bottom)
