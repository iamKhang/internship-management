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
        this._originalStaticOptions = null; // [{value,text}]
        
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
        
        // Capture initial static options (if any) for local filtering mode
        this._captureInitialStaticOptions();
        
        this.setupEventListeners();
        this.loadInitialData();
        // Ensure static DOM options (if any) are clickable even without remote search
        this.bindDomOptions();
        // Enable combobox behavior by default
        this._setupComboboxBehavior();
    }
    
    setupEventListeners() {
        // Input focus
        this.textInput.addEventListener('focus', () => {
            // Do not clear default text on focus; allow click selection to replace it
            if (!this.isDropdownOpen) {
                this.openDropdown();
            }
        });
        
        // Select-all on first click (before caret placement)
        this.textInput.addEventListener('mousedown', (e) => {
            const notFocused = document.activeElement !== this.textInput;
            const fullSelected = this.textInput.selectionStart === 0 && this.textInput.selectionEnd === this.textInput.value.length;
            if (notFocused || !fullSelected) {
                e.preventDefault();
                this.textInput.focus();
                this.textInput.select();
            }
        });
        
        // Input typing
        this.textInput.addEventListener('input', (e) => {
            this.currentSearchQuery = e.target.value;
            
            // Clear selection if user is typing something different
            if (this.hiddenInput.value && e.target.value !== this.hiddenInput.value) {
                this.hiddenInput.value = '';
            }
            
            // When typing, do not auto-revert to default; just perform search/filter
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
        
        // Input click (open dropdown if needed)
        this.textInput.addEventListener('click', () => {
            if (!this.isDropdownOpen) {
                this.openDropdown();
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

    bindDomOptions() {
        const items = this.optionsContainer.querySelectorAll('.dropdown-item');
        if (!items || items.length === 0) return;
        items.forEach((item, idx) => {
            // Avoid double-binding
            if (item._sdBound) return;
            item._sdBound = true;
            item.addEventListener('click', (e) => {
                e.preventDefault();
                this.selectOption(idx);
            });
        });
    }
    
    async searchData(query = '', filterValue = '') {
        if (!this.options.searchUrl) {
            // Static DOM mode: perform client-side filtering using captured options
            const base = Array.isArray(this._originalStaticOptions) ? this._originalStaticOptions : this._readCurrentDomOptions();
            const q = String(query || '').toLowerCase();
            const filtered = base.filter(o => !q || (o.text || '').toLowerCase().includes(q) || String(o.value || '').toLowerCase().includes(q));
            await this.updateOptions(filtered);
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
        
        // Always run a search/filter pass when opening to reflect current query
        this.searchData(this.currentSearchQuery, this.getFilterValue());
    }
    
    closeDropdown() {
        this.dropdown.style.display = 'none';
        if (this.arrow) {
            this.arrow.classList.remove('rotated');
        }
        this.isDropdownOpen = false;
        this.selectedIndex = -1;
        this.clearSelection();
        
        // On close, if there is no selected value, revert to default display
        if (!this.hiddenInput.value) {
            this._applyDefaultDisplay();
        }
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
    
    // Helper to find and select default option
    _findAndSelectDefaultOption() {
        const firstOption = this.optionsContainer.querySelector('.dropdown-item[data-value=""]') ||
                           this.optionsContainer.querySelector('.dropdown-item[data-value="All"]') ||
                           this.optionsContainer.querySelector('.dropdown-item');
        if (firstOption) {
            const defaultValue = firstOption.getAttribute('data-value') || '';
            const defaultText = firstOption.textContent || '-- Tất cả --';
            this.hiddenInput.value = defaultValue;
            this.textInput.value = defaultText;
        } else {
            // Fallback text
            this.hiddenInput.value = '';
            this.textInput.value = '-- Tất cả --';
        }
    }

    _applyDefaultDisplay() {
        // Ensure the text input shows default display when no selection
        const defaultText = this._getDefaultText();
        this.textInput.value = defaultText;
    }

    _getDefaultText() {
        const defaultAnchor = this.optionsContainer.querySelector('.dropdown-item[data-value=""]');
        return defaultAnchor ? (defaultAnchor.textContent || '-- Tất cả --') : '-- Tất cả --';
    }

    _isDefaultDisplay(text) {
        const t = String(text || '').trim();
        const def = String(this._getDefaultText() || '').trim();
        return t === def;
    }
    
    // Ensure combobox never stays with arbitrary text when leaving without a selection
    _setupComboboxBehavior() {
        // Do NOT auto-revert to default on input while typing; only on blur/close if no selection
        this.textInput.addEventListener('blur', (e) => {
            if (!this.hiddenInput.value) {
                this._applyDefaultDisplay();
            }
        });
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
        
        if (this.options.searchUrl && filterValue) {
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
            } else {
                // No initial value; show default text
                this._applyDefaultDisplay();
            }
        } else if (this.options.searchUrl && initialValue) {
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
        } else {
            // Static DOM mode: reflect initial hidden value to text if possible, otherwise default
            if (initialValue) {
                const selectedOption = this.optionsContainer.querySelector(`[data-value="${initialValue}"]`);
                if (selectedOption) {
                    this.textInput.value = selectedOption.textContent;
                } else {
                    this._applyDefaultDisplay();
                }
            } else {
                this._applyDefaultDisplay();
            }
            this.bindDomOptions();
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

    _readCurrentDomOptions() {
        const all = [];
        this.optionsContainer.querySelectorAll('.dropdown-item').forEach(a => {
            const value = a.getAttribute('data-value');
            const text = a.textContent || '';
            if (value !== null) all.push({ value, text });
        });
        // Remove the default option from base (value == '') to avoid duplicate when filtering
        return all.filter(o => String(o.value) !== '');
    }

    _captureInitialStaticOptions() {
        // Build a base list of options from initial DOM for static filtering
        const opts = [];
        const anchors = this.optionsContainer ? this.optionsContainer.querySelectorAll('.dropdown-item') : [];
        anchors.forEach(a => {
            const v = a.getAttribute('data-value');
            if (v === null) return;
            const t = a.textContent || '';
            opts.push({ value: v, text: t });
        });
        this._originalStaticOptions = opts.filter(o => String(o.value) !== '');
    }
}

// Export for use in modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = SearchableDropdown;
}
