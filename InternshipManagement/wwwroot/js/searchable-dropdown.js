/**
 * SearchableDropdown - A reusable searchable dropdown component
 * Usage: new SearchableDropdown(elementId, options)
 */
class SearchableDropdown {
    constructor(elementId, options = {}) {
        this.elementId = elementId;
        this.options = {
            placeholder: 'Tìm kiếm...',
            searchUrl: '',
            searchParam: 'q',
            filterParam: 'filter',
            debounceDelay: 300,
            minSearchLength: 0,
            allowClear: true,
            ...options
        };
        
        this.isDropdownOpen = false;
        this.currentSearchQuery = '';
        this.selectedIndex = -1;
        
        this.init();
    }
    
    init() {
        this.container = document.getElementById(this.elementId);
        if (!this.container) {
            console.error(`Element with id "${this.elementId}" not found`);
            return;
        }
        
        this.hiddenInput = this.container.querySelector('input[type="hidden"]');
        this.textInput = this.container.querySelector('input[type="text"]');
        this.dropdown = this.container.querySelector('.dropdown-menu');
        this.optionsContainer = this.container.querySelector('.dropdown-options');
        this.arrow = this.container.querySelector('.dropdown-arrow');
        
        if (!this.hiddenInput || !this.textInput || !this.dropdown || !this.optionsContainer) {
            console.error('Required elements not found in SearchableDropdown container');
            return;
        }
        
        this.setupEventListeners();
        this.loadInitialData();
    }
    
    setupEventListeners() {
        // Input focus
        this.textInput.addEventListener('focus', () => {
            if (!this.isDropdownOpen) {
                this.openDropdown();
            }
        });
        
        // Input typing
        this.textInput.addEventListener('input', (e) => {
            this.currentSearchQuery = e.target.value;
            
            // Clear selection if user is typing something different
            if (this.hiddenInput.value && e.target.value !== this.hiddenInput.value) {
                this.hiddenInput.value = '';
            }
            
            if (this.isDropdownOpen) {
                this.debouncedSearch(e.target.value);
            } else {
                this.openDropdown();
            }
        });
        
        // Keyboard navigation
        this.textInput.addEventListener('keydown', (e) => {
            this.handleKeyboardNavigation(e);
        });
        
        // Input click
        this.textInput.addEventListener('click', () => {
            if (!this.isDropdownOpen) {
                this.openDropdown();
            }
            // Select all text when clicking on input with existing value
            if (this.textInput.value && this.hiddenInput.value) {
                this.textInput.select();
            }
        });
        
        // Close dropdown when clicking outside
        document.addEventListener('click', (e) => {
            if (!this.container.contains(e.target)) {
                this.closeDropdown();
            }
        });
    }
    
    handleKeyboardNavigation(e) {
        if (!this.isDropdownOpen) {
            if (e.key === 'ArrowDown' || e.key === 'Enter') {
                e.preventDefault();
                this.openDropdown();
                return;
            }
        }

        const options = this.optionsContainer.querySelectorAll('.dropdown-item');
        
        switch (e.key) {
            case 'ArrowDown':
                e.preventDefault();
                if (this.selectedIndex < options.length - 1) {
                    this.selectedIndex++;
                    this.updateSelection();
                }
                break;
                
            case 'ArrowUp':
                e.preventDefault();
                if (this.selectedIndex > 0) {
                    this.selectedIndex--;
                    this.updateSelection();
                }
                break;
                
            case 'Enter':
                e.preventDefault();
                if (this.selectedIndex >= 0 && this.selectedIndex < options.length) {
                    this.selectOption(this.selectedIndex);
                }
                break;
                
            case 'Escape':
                e.preventDefault();
                this.closeDropdown();
                break;
                
            case 'Tab':
                // Allow normal tab behavior
                break;
                
            default:
                // For other keys, let the input handle them normally
                break;
        }
    }
    
    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
    
    async updateOptions(data) {
        // Clear current options
        this.optionsContainer.innerHTML = '';

        // Add default option if allowClear is true
        if (this.options.allowClear) {
            const defaultOption = document.createElement('a');
            defaultOption.className = 'dropdown-item';
            defaultOption.href = '#';
            defaultOption.setAttribute('data-value', '');
            defaultOption.textContent = '-- Tất cả --';
            this.optionsContainer.appendChild(defaultOption);
        }

        // Add new options
        if (data && data.length > 0) {
            data.forEach(option => {
                const opt = document.createElement('a');
                opt.className = 'dropdown-item';
                opt.href = '#';

                // Allow custom label/value formatter from options
                if (typeof this.options.labelFormatter === 'function') {
                    const formatted = this.options.labelFormatter(option) || {};
                    const value = formatted.value ?? '';
                    const text = formatted.text ?? '';
                    opt.setAttribute('data-value', value);
                    opt.textContent = text;
                    this.optionsContainer.appendChild(opt);
                    return;
                }

                // Handle different data formats (default behaviors)
                let value, text;
                if (option.value !== undefined && option.text !== undefined) {
                    // Standard format: {value, text}
                    value = option.value;
                    text = option.text;
                } else if (
                    (option.MaGv !== undefined && option.HoTenGv !== undefined) ||
                    (option.maGv !== undefined && option.hoTenGv !== undefined)
                ) {
                    // GiangVien format: PascalCase or camelCase
                    const mgv = option.MaGv ?? option.maGv;
                    const hten = option.HoTenGv ?? option.hoTenGv;
                    value = String(mgv);
                    text = `${hten} - ${mgv}`;
                } else if (
                    (option.MaKhoa !== undefined && option.TenKhoa !== undefined) ||
                    (option.maKhoa !== undefined && option.tenKhoa !== undefined)
                ) {
                    // Khoa format: PascalCase or camelCase
                    const mk = option.MaKhoa ?? option.maKhoa;
                    const tk = option.TenKhoa ?? option.tenKhoa;
                    value = mk;
                    text = `${tk}`;
                } else {
                    // Fallback: use first two properties
                    const keys = Object.keys(option);
                    value = option[keys[0]] || '';
                    text = option[keys[1]] || option[keys[0]] || '';
                }

                opt.setAttribute('data-value', value);
                opt.textContent = text;
                this.optionsContainer.appendChild(opt);
            });
        } else {
            const noResultOption = document.createElement('a');
            noResultOption.className = 'dropdown-item text-muted';
            noResultOption.href = '#';
            noResultOption.textContent = 'Không tìm thấy kết quả';
            this.optionsContainer.appendChild(noResultOption);
        }

        // Add click handlers to new options
        this.optionsContainer.querySelectorAll('.dropdown-item').forEach((item, index) => {
            item.addEventListener('click', (e) => {
                e.preventDefault();
                this.selectOption(index);
            });
        });
    }
    
    async searchData(query = '', filterValue = '') {
        if (!this.options.searchUrl) {
            console.warn('Search URL not provided');
            return;
        }
        
        const url = new URL(this.options.searchUrl, window.location.origin);
        url.searchParams.set(this.options.searchParam, query);
        if (filterValue) {
            url.searchParams.set(this.options.filterParam, filterValue);
        }
        
        try {
            const response = await fetch(url);
            const data = await response.json();
            
            // Handle different response formats
            let options = [];
            if (data.success && data.data) {
                // API format: {success: true, data: [...]}
                options = data.data;
            } else if (data.options) {
                options = data.options;
            } else if (Array.isArray(data)) {
                options = data;
            } else if (data.results) {
                options = data.results;
            }
            
            await this.updateOptions(options);
        } catch (error) {
            console.error('Error searching data:', error);
            this.optionsContainer.innerHTML = '<a class="dropdown-item text-danger" href="#">Lỗi khi tải dữ liệu</a>';
        }
    }
    
    openDropdown() {
        this.dropdown.style.display = 'block';
        if (this.arrow) {
            this.arrow.classList.add('rotated');
        }
        this.isDropdownOpen = true;
        this.selectedIndex = -1;
        this.clearSelection();
        
        // Load initial data if not loaded
        if (this.optionsContainer.children.length <= (this.options.allowClear ? 1 : 0)) {
            this.searchData(this.currentSearchQuery, this.getFilterValue());
        }
    }
    
    closeDropdown() {
        this.dropdown.style.display = 'none';
        if (this.arrow) {
            this.arrow.classList.remove('rotated');
        }
        this.isDropdownOpen = false;
        this.selectedIndex = -1;
        this.clearSelection();
    }
    
    selectOption(index) {
        const options = this.optionsContainer.querySelectorAll('.dropdown-item');
        if (options[index]) {
            const value = options[index].getAttribute('data-value');
            const text = options[index].textContent;
            
            if (value !== null) {
                this.hiddenInput.value = value;
                this.textInput.value = text;
                this.closeDropdown();
                
                // Trigger custom event
                this.container.dispatchEvent(new CustomEvent('selectionChanged', {
                    detail: { value, text }
                }));
            }
        }
    }
    
    clearSelection() {
        this.optionsContainer.querySelectorAll('.dropdown-item').forEach(item => {
            item.classList.remove('active');
        });
    }
    
    updateSelection() {
        this.clearSelection();
        const options = this.optionsContainer.querySelectorAll('.dropdown-item');
        if (this.selectedIndex >= 0 && this.selectedIndex < options.length) {
            options[this.selectedIndex].classList.add('active');
            options[this.selectedIndex].scrollIntoView({ block: 'nearest' });
        }
    }
    
    getFilterValue() {
        // Override this method in child classes or provide filterValue in options
        return this.options.filterValue || '';
    }
    
    setFilterValue(value) {
        this.options.filterValue = value;
        this.hiddenInput.value = '';
        this.textInput.value = '';
        this.currentSearchQuery = '';
        
        // Always update data when filter changes, regardless of dropdown state
        this.searchData('', value);
    }
    
    setValue(value, text) {
        this.hiddenInput.value = value;
        this.textInput.value = text || '';
    }
    
    getValue() {
        return this.hiddenInput.value;
    }
    
    getText() {
        return this.textInput.value;
    }
    
    clear() {
        this.hiddenInput.value = '';
        this.textInput.value = '';
        this.currentSearchQuery = '';
    }
    
    async loadInitialData() {
        const initialValue = this.hiddenInput.value;
        const filterValue = this.getFilterValue();
        
        if (filterValue) {
            await this.searchData('', filterValue);
            
            if (initialValue) {
                const selectedOption = this.optionsContainer.querySelector(`[data-value="${initialValue}"]`);
                if (selectedOption) {
                    this.textInput.value = selectedOption.textContent;
                } else {
                    // Try searching for the specific value
                    await this.searchData(initialValue, filterValue);
                    const foundOption = this.optionsContainer.querySelector(`[data-value="${initialValue}"]`);
                    if (foundOption) {
                        this.textInput.value = foundOption.textContent;
                    } else {
                        this.textInput.value = `#${initialValue}`;
                    }
                }
            }
        } else if (initialValue) {
            // If no filter but there's a value, load all data to find the selected one
            await this.searchData('', '');
            const selectedOption = this.optionsContainer.querySelector(`[data-value="${initialValue}"]`);
            if (selectedOption) {
                this.textInput.value = selectedOption.textContent;
            } else {
                // Try searching for the specific value
                await this.searchData(initialValue, '');
                const foundOption = this.optionsContainer.querySelector(`[data-value="${initialValue}"]`);
                if (foundOption) {
                    this.textInput.value = foundOption.textContent;
                } else {
                    this.textInput.value = `#${initialValue}`;
                }
            }
        }
    }
    
    // Initialize debounced search
    get debouncedSearch() {
        if (!this._debouncedSearch) {
            this._debouncedSearch = this.debounce((query) => {
                this.searchData(query, this.getFilterValue());
            }, this.options.debounceDelay);
        }
        return this._debouncedSearch;
    }
}

// Export for use in modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = SearchableDropdown;
}
