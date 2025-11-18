import React, { useEffect, useRef, useState } from 'react';

// Lightweight in-app debug console for diagnosing Teams auth issues.
// Captures console.log/warn/error, window.onerror, unhandledrejection.
// Toggle visibility with Ctrl+Alt+D or the floating button.
export default function DebugConsole({ maxEntries = 500 }) {
  const [open, setOpen] = useState(false);
  const [entries, setEntries] = useState([]);
  const [filter, setFilter] = useState('');
  const orig = useRef({});

  useEffect(() => {
    // Patch console methods
    const levels = ['log','warn','error','info','debug'];
    const originalSnapshot = {};
    levels.forEach(level => {
      if (!orig.current[level]) orig.current[level] = console[level];
      originalSnapshot[level] = orig.current[level];
      console[level] = function patched(...args) {
        try {
          const time = new Date().toISOString();
          const text = args.map(a => {
            if (a instanceof Error) return `${a.name}: ${a.message}`;
            if (typeof a === 'object') {
              try { return JSON.stringify(a); } catch { return String(a); }
            }
            return String(a);
          }).join(' ');
          setEntries(prev => {
            const next = [...prev, { level, text, time }];
            return next.length > maxEntries ? next.slice(next.length - maxEntries) : next;
          });
        } catch {/* ignore */}
        orig.current[level](...args);
      };
    });

    const handleError = (msg, src, line, col, err) => {
      const time = new Date().toISOString();
      setEntries(prev => [...prev, { level: 'error', text: `[window.onerror] ${msg} (${src}:${line}:${col}) ${err?.message || ''}`, time }]);
    };
    const handleRejection = evt => {
      const reason = evt.reason instanceof Error ? `${evt.reason.name}: ${evt.reason.message}` : String(evt.reason);
      const time = new Date().toISOString();
      setEntries(prev => [...prev, { level: 'error', text: `[unhandledrejection] ${reason}`, time }]);
    };
    window.addEventListener('error', handleError);
    window.addEventListener('unhandledrejection', handleRejection);

    const keyHandler = e => {
      if (e.ctrlKey && e.altKey && e.key.toLowerCase() === 'd') {
        setOpen(o => !o);
      }
    };
    window.addEventListener('keydown', keyHandler);

    return () => {
      window.removeEventListener('error', handleError);
      window.removeEventListener('unhandledrejection', handleRejection);
      window.removeEventListener('keydown', keyHandler);
      // restore console from snapshot
      Object.entries(originalSnapshot).forEach(([k,v]) => { console[k] = v; });
    };
  }, [maxEntries]);

  const filtered = entries.filter(e => !filter || e.text.toLowerCase().includes(filter.toLowerCase()));

  function copyAll() {
    const blob = filtered.map(e => `${e.time} [${e.level}] ${e.text}`).join('\n');
    navigator.clipboard.writeText(blob).catch(() => {});
  }
  function clearAll() { setEntries([]); }

  return (
    <>
      <button
        type="button"
        onClick={() => setOpen(o => !o)}
        style={{ position:'fixed', bottom:8, right:8, zIndex:9999 }}
        className="px-2 py-1 text-xs bg-gray-800 text-white rounded shadow"
        title="Toggle debug console (Ctrl+Alt+D)"
      >{open ? 'Hide Debug' : 'Show Debug'}</button>
      {open && (
        <div className="fixed bottom-0 left-0 right-0 h-64 bg-white border-t border-gray-300 shadow-xl z-[9998] flex flex-col text-xs font-mono">
          <div className="flex items-center gap-2 p-1 bg-gray-100 border-b border-gray-300">
            <strong className="mr-auto">Debug Console</strong>
            <input
              placeholder="Filter"
              value={filter}
              onChange={e => setFilter(e.target.value)}
              className="border px-1 py-0.5 rounded"
              style={{ minWidth:120 }}
            />
            <button onClick={copyAll} className="border px-1 py-0.5 rounded bg-gray-200">Copy</button>
            <button onClick={clearAll} className="border px-1 py-0.5 rounded bg-gray-200">Clear</button>
          </div>
          <div className="flex-1 overflow-auto p-1" data-testid="debug-log-scroll">
            {filtered.length === 0 && <div className="text-gray-400">(no log entries)</div>}
            {filtered.map((e,i) => (
              <div key={i} className={`mb-0.5 ${e.level==='error'?'text-red-600': e.level==='warn'? 'text-amber-600':'text-gray-800'}`}>[{e.time}] [{e.level}] {e.text}</div>
            ))}
          </div>
        </div>
      )}
    </>
  );
}
