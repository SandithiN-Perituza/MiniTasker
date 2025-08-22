export default function Header({ onMenuClick }) {
  return (
    <header className="bg-blue-600 text-white p-4 flex items-center justify-between">
      <button
        className="mr-2 focus:outline-none"
        onClick={onMenuClick}
        aria-label="Open menu"
      >
        <svg width="28" height="28" fill="none" stroke="currentColor" strokeWidth="2">
          <path d="M4 7h20M4 14h20M4 21h20" />
        </svg>
      </button>
      <div className="flex items-center justify-center flex-1 gap-2">
        <img src="/logo.png" alt="Mini Tasker Logo" className="h-8" />
        <span className="text-2xl font-bold">Mini&nbsp;Tasker</span>
      </div>
    </header>
  );
}
