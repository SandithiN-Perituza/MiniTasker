export default function Footer() {
  return (
    <footer className="text-white bg-blue-700 border-t border-gray-200 text-center p-3">
      &copy; {new Date().getFullYear()} MiniTasker
    </footer>
  );
}
