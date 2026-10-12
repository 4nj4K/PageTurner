const API_URL = '/api/books';

// DOM Elements
const booksGrid = document.getElementById('books-grid');
const searchInput = document.getElementById('search-input');
const statusFilter = document.getElementById('status-filter');
const sortBySelect = document.getElementById('sort-by');

const modalOverlay = document.getElementById('modal-overlay');
const openModalBtn = document.getElementById('open-modal-btn');
const closeModalBtn = document.getElementById('close-modal-btn');
const addBookForm = document.getElementById('add-book-form');

// Init
document.addEventListener('DOMContentLoaded', () => {
    fetchBooks();
    fetchStats();
});

// Event Listeners for Filtering & Search
searchInput.addEventListener('input', fetchBooks);
statusFilter.addEventListener('change', fetchBooks);
sortBySelect.addEventListener('change', fetchBooks);

// Modal Open/Close
openModalBtn.addEventListener('click', () => modalOverlay.classList.remove('hidden'));
closeModalBtn.addEventListener('click', () => modalOverlay.classList.add('hidden'));
modalOverlay.addEventListener('click', (e) => {
    if (e.target === modalOverlay) modalOverlay.classList.add('hidden');
});

// Fetch Stats from API
async function fetchStats() {
    try {
        const response = await fetch(`${API_URL}/stats`);
        if (!response.ok) throw new Error('Failed to fetch stats');
        const stats = await response.json();

        document.getElementById('stat-total').textContent = stats.totalBooks;
        document.getElementById('stat-finished').textContent = stats.finished;
        document.getElementById('stat-reading').textContent = stats.currentlyReading;
        document.getElementById('stat-want').textContent = stats.wantToRead;
        document.getElementById('stat-avg').textContent = stats.averageRating;
    } catch (err) {
        console.error('Error loading stats:', err);
    }
}

// Fetch Books from API
async function fetchBooks() {
    const search = searchInput.value.trim();
    const status = statusFilter.value;
    const sortBy = sortBySelect.value;

    const params = new URLSearchParams();
    if (search) params.append('search', search);
    if (status) params.append('status', status);
    if (sortBy) params.append('sortBy', sortBy);

    try {
        const response = await fetch(`${API_URL}?${params.toString()}`);
        if (!response.ok) throw new Error('Failed to fetch books');
        const books = await response.json();

        renderBooks(books);
    } catch (err) {
        console.error('Error loading books:', err);
        booksGrid.innerHTML = `<p style="color: var(--danger);">Failed to load books. Make sure server is running.</p>`;
    }
}

// Render Book Cards
function renderBooks(books) {
    if (books.length === 0) {
        booksGrid.innerHTML = `<p style="color: var(--text-muted); grid-column: 1/-1; text-align: center; padding: 2rem;">No books found matching your criteria.</p>`;
        return;
    }

    booksGrid.innerHTML = books.map(book => {
        let badgeClass = 'badge-want';
        let statusText = 'Want to Read';

        if (book.status === 'Finished') {
            badgeClass = 'badge-finished';
            statusText = 'Finished';
        } else if (book.status === 'Reading') {
            badgeClass = 'badge-reading';
            statusText = 'Currently Reading';
        }

        const ratingStars = book.rating ? '⭐'.repeat(book.rating) : 'Unrated';
        
        // HÄR lägger vi till anteckningen om den finns!
        const notesSection = book.notes ? `<p class="book-author" style="font-style: italic; margin-top: 0.5rem;">"${escapeHtml(book.notes)}"</p>` : '';

        return `
            <div class="book-card">
                <div>
                    <div class="book-header">
                        <h3 class="book-title">${escapeHtml(book.title)}</h3>
                        <span class="badge ${badgeClass}">${statusText}</span>
                    </div>
                    <p class="book-author">by ${escapeHtml(book.author)}</p>
                    
                    <!-- Här skrivs anteckningen ut på kortet -->
                    ${notesSection}

                    <div class="book-meta">
                        <span>Genre: ${escapeHtml(book.genre || 'N/A')}</span>
                        <span>Rating: ${ratingStars}</span>
                    </div>
                </div>
                <div class="book-actions">
                    <button class="btn btn-danger" onclick="deleteBook(${book.id})">Delete</button>
                </div>
            </div>
        `;
    }).join('');
}

// Add New Book
addBookForm.addEventListener('submit', async (e) => {
    e.preventDefault();

    const newBook = {
        title: document.getElementById('title').value,
        author: document.getElementById('author').value,
        genre: document.getElementById('genre').value || null,
        status: document.getElementById('status').value,
        rating: document.getElementById('rating').value ? parseInt(document.getElementById('rating').value) : null
        notes: document.getElementById('notes').value || null
    };

    try {
        const response = await fetch(API_URL, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(newBook)
        });

        if (!response.ok) throw new Error('Failed to create book');

        addBookForm.reset();
        modalOverlay.classList.add('hidden');
        fetchBooks();
        fetchStats();
    } catch (err) {
        console.error('Error adding book:', err);
        alert('Could not add book. Check console for details.');
    }
});

// Delete Book
async function deleteBook(id) {
    if (!confirm('Are you sure you want to delete this book?')) return;

    try {
        const response = await fetch(`${API_URL}/${id}`, { method: 'DELETE' });
        if (!response.ok) throw new Error('Failed to delete book');

        fetchBooks();
        fetchStats();
    } catch (err) {
        console.error('Error deleting book:', err);
    }
}

// Helper to prevent XSS
function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/[&<>"']/g, match => ({
        '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
    }[match]));
}