import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

export const SearchBar = () => {
  const [query, setQuery] = useState('');
  const navigate = useNavigate();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (query.trim()) {
      navigate(`/search?q=${encodeURIComponent(query.trim())}`);
    }
  };

  return (
    <form onSubmit={handleSubmit} style={{ display: 'flex', gap: 8 }}>
      <input
        type="text"
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        placeholder="Search products..."
        style={{ padding: '6px 12px', borderRadius: 6, border: '1px solid #cbd5e0', minWidth: 250 }}
      />
      <button type="submit" style={{ padding: '6px 16px', background: '#3182ce', color: 'white', border: 'none', borderRadius: 6, cursor: 'pointer', fontWeight: 600 }}>
        Search
      </button>
    </form>
  );
};
