import React from 'react';

const Privacy = () => (
    <div className="max-w-3xl mx-auto my-10 p-6 bg-white rounded-lg shadow-md">
        <h1 className="text-2xl font-bold mb-4">Privacy Policy</h1>
        <p className="mb-4">
            MiniTasker values your privacy. This Privacy Policy explains how we collect, use, and protect your information when you use our app.
        </p>
        <h2 className="text-xl font-semibold mt-6 mb-2">Information We Collect</h2>
        <ul className="list-disc list-inside mb-4">
            <li>Account information (such as email address)</li>
            <li>Task data you create or manage</li>
            <li>Usage data to improve our services</li>
        </ul>
        <h2 className="text-xl font-semibold mt-6 mb-2">How We Use Your Information</h2>
        <ul className="list-disc list-inside mb-4">
            <li>To provide and improve our services</li>
            <li>To communicate with you about updates or support</li>
            <li>To ensure security and prevent misuse</li>
        </ul>
        <h2 className="text-xl font-semibold mt-6 mb-2">Data Protection</h2>
        <p className="mb-4">
            We implement security measures to protect your data. Your information is not shared with third parties except as required by law.
        </p>
        <h2 className="text-xl font-semibold mt-6 mb-2">Your Choices</h2>
        <p className="mb-4">
            You can access, update, or delete your account information at any time. Contact us for any privacy-related concerns.
        </p>
        <h2 className="text-xl font-semibold mt-6 mb-2">Contact Us</h2>
        <p className="mb-4">
            If you have questions about this Privacy Policy, please email us at <a className="text-blue-500">support@minitasker.com</a>. {/* href="mailto:support@minitasker.com" */}
        </p>
        <p className="mt-8 text-sm text-gray-500">
            Last updated: September 2025
        </p>
    </div>
);

export default Privacy;