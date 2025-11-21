import React from 'react';
import reportError from '../utils/errorReporter';

export default class ErrorBoundary extends React.Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error) {
    return { hasError: true, error };
  }

  componentDidCatch(error, info) {
    console.error('Unhandled error caught by ErrorBoundary', error, info);
    reportError({ message: error?.message || String(error), stack: info?.componentStack, source: 'frontend' });
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="p-4 bg-red-50 text-red-700 rounded">
          <h2 className="font-bold">Something went wrong</h2>
          <p>We've recorded the error and will investigate.</p>
        </div>
      );
    }

    return this.props.children;
  }
}
