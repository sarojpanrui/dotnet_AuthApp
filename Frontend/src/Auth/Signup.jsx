import React, { useState } from "react";
import axios from "axios";

const Signup = () => {
  const [formData, setFormData] = useState({
    username: "",
    email: "",
    password: "",
    role: "", // default to customer as number
    phone: "",
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);

  const handleChange = (e) => {
    const { name, value } = e.target;



    // Convert role to number
    setFormData({
      ...formData,
      [name]: value
    });

    console.log(formData)
  };

  const handleSubmit = async (e) => {

    e.preventDefault();
    console.log(e.target.value);
    setLoading(true);
    setError("");
    setSuccess(false);

    try {
      // Send payload as-is (role is already number)
      const payload = {
        ...formData,
        role:Number(formData.role)

      }

      console.log(payload);
      const response = await axios.post("http://localhost:5066/api/Auth/register", payload);

      console.log("Signup Success:", response.data);
      setSuccess(true);

      // Reset form
      setFormData({
        username: "",
        email: "",
        password: "",
        role: "",
        phone: "",
      });
    } catch (err) {
      setError(err.response?.data?.message || "Signup failed. Try again.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex h-screen w-screen items-center justify-center bg-gradient-to-r from-green-400 via-blue-500 to-purple-600">
      <div className="w-full max-w-lg bg-white p-8 rounded-2xl shadow-2xl">
        <h1 className="text-3xl font-bold text-center text-gray-800 mb-6">
          Create an Account ✨
        </h1>

        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Username */}
          <div>
            <label className="block text-gray-600 font-medium mb-1">Username</label>
            <input
              type="text"
              name="username"
              value={formData.username}
              onChange={handleChange}
              placeholder="Enter your username"
              required
              className="w-full px-4 py-2 border rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-400"
            />
          </div>

          {/* Email */}
          <div>
            <label className="block text-gray-600 font-medium mb-1">Email</label>
            <input
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              placeholder="Enter your email"
              required
              className="w-full px-4 py-2 border rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-400"
            />
          </div>

          {/* Password */}
          <div>
            <label className="block text-gray-600 font-medium mb-1">Password</label>
            <input
              type="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              placeholder="Enter your password"
              required
              className="w-full px-4 py-2 border rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-400"
            />
          </div>

          {/* Role */}
          <div>
            <label className="block text-gray-600 font-medium mb-1">Role</label>
            <select
              name="role"
              value={formData.role}
              onChange={handleChange}
              className="w-full px-4 py-2 border rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-400"
            >
              <option value={0}>Admin</option>
              <option value={1}>Customer</option>
            </select>
          </div>

          {/* Phone */}
          <div>
            <label className="block text-gray-600 font-medium mb-1">Phone</label>
            <input
              type="tel"
              name="phone"
              value={formData.phone}
              onChange={handleChange}
              placeholder="Enter your phone number"
              required
              className="w-full px-4 py-2 border rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-400"
            />
          </div>

          {/* Error / Success */}
          {error && <p className="text-red-500 text-sm">{error}</p>}
          {success && <p className="text-green-500 text-sm">Signup Successful 🎉</p>}

          {/* Submit Button */}
          <button
            type="submit"
            disabled={loading}
            className="w-full bg-blue-600 hover:bg-blue-700 text-white py-2 rounded-xl font-semibold transition duration-200"
          >
            {loading ? "Signing up..." : "Sign Up"}
          </button>
        </form>

        <p className="text-center text-gray-500 text-sm mt-6">
          Already have an account?{" "}
          <a href="/login" className="text-blue-600 hover:underline">Login</a>
        </p>
      </div>
    </div>
  );
};

export default Signup;
